# Лабораторна: масштабування backend у Kubernetes

Ця лабораторна показує горизонтальне масштабування backend для MediDispense у локальному Kubernetes кластері Docker Desktop.

Система в Kubernetes складається з:

- PostgreSQL pod з постійним сховищем;
- ASP.NET Core API Deployment, який можна масштабувати від 1 до кількох pod-ів;
- Kubernetes LoadBalancer Service на `localhost:8080`;
- HorizontalPodAutoscaler для автоматичного масштабування API за CPU-навантаженням;
- Locust UI для навантажувального тестування на `localhost:8089`.

## Що показати на захисті

1. API працює через Kubernetes Service, а не напряму з IDE.
2. При `replicas=1` є один backend pod.
3. Після `kubectl scale ... --replicas=3` з'являються три backend pod-и.
4. Запити до `/api/diagnostics/instance` повертають різні pod names.
5. Locust показує більший RPS при більшій кількості API pod-ів.

## Передумови

- Docker Desktop з увімкненим Kubernetes.
- `kubectl` налаштований на Docker Desktop кластер.
- .NET 9 SDK для локальної перевірки збірки.

Перевірити кластер:

```powershell
kubectl config current-context
kubectl get nodes
```

Очікуваний context зазвичай називається `docker-desktop`.

## Збірка Docker image

Виконати з кореня репозиторію:

```powershell
docker build -t medidispense-api:lab .
```

У Kubernetes для API вказано `imagePullPolicy: Never`, тому image `medidispense-api:lab` має бути зібраний локально перед запуском pod-ів.

Якщо Docker Desktop використовує Kubernetes provider `kind` і API pod показує `ErrImageNeverPull`, треба завантажити локальний image у Kubernetes node:

```powershell
docker save medidispense-api:lab -o medidispense-api-lab.tar
docker cp medidispense-api-lab.tar desktop-control-plane:/medidispense-api-lab.tar
docker exec desktop-control-plane ctr -n k8s.io images import /medidispense-api-lab.tar
Remove-Item -LiteralPath medidispense-api-lab.tar
kubectl delete pod -l app=medidispense-api
kubectl wait --for=condition=available deployment/medidispense-api --timeout=120s
```

## Деплой у Kubernetes

Застосувати manifests у такому порядку:

```powershell
kubectl apply -f k8s/postgres.yaml
kubectl wait --for=condition=available deployment/medidispense-postgres --timeout=120s

kubectl apply -f k8s/api.yaml
kubectl apply -f k8s/load-balancer.yaml
kubectl wait --for=condition=available deployment/medidispense-api --timeout=120s
```

Перевірити ресурси:

```powershell
kubectl get pods
kubectl get svc
```

Якщо Docker Desktop не відкрив LoadBalancer на `localhost:8080`, можна тимчасово використати port-forward:

```powershell
kubectl port-forward service/medidispense-api 8080:8080
```

## Перевірка API

Health check:

```powershell
Invoke-RestMethod http://localhost:8080/health
```

Показати, який pod обробив запит:

```powershell
Invoke-RestMethod http://localhost:8080/api/diagnostics/instance
```

Увійти під seed admin користувачем:

```powershell
$login = Invoke-RestMethod `
  -Method Post `
  -Uri http://localhost:8080/api/auth/login `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"admin"}'

$headers = @{ Authorization = "Bearer $($login.token)" }
Invoke-RestMethod -Uri http://localhost:8080/api/medications -Headers $headers
```

## Масштабування API

Запустити один backend pod:

```powershell
kubectl scale deployment medidispense-api --replicas=1
kubectl rollout status deployment/medidispense-api
kubectl get pods
```

Збільшити кількість backend pod-ів до трьох:

```powershell
kubectl scale deployment medidispense-api --replicas=3
kubectl rollout status deployment/medidispense-api
kubectl get pods
```

Кілька разів викликати endpoint. Значення `instance` має змінюватися між назвами pod-ів:

```powershell
1..10 | ForEach-Object {
  curl.exe -s -H "Connection: close" http://localhost:8080/api/diagnostics/instance
}
```

`curl.exe` з `Connection: close` відкриває нове з'єднання для кожного запиту, тому Kubernetes Service краще показує розподіл між pod-ами.

Повернутися до одного pod-а:

```powershell
kubectl scale deployment medidispense-api --replicas=1
kubectl rollout status deployment/medidispense-api
```

## Автоматичне масштабування API

Для автоматичного масштабування використовується Kubernetes `HorizontalPodAutoscaler`:

```powershell
kubectl apply -f k8s/hpa.yaml
kubectl get hpa
```

HPA налаштований так:

- мінімум: `1` API pod;
- максимум: `5` API pod-ів;
- цільове CPU-навантаження: `50%`;
- scale up виконується швидко, щоб це було зручно показати на лабораторній;
- scale down має коротке вікно стабілізації `60s`.

Для роботи HPA у кластері має бути `metrics-server`. Перевірити:

```powershell
kubectl top pods
```

Якщо команда не працює, встановити `metrics-server`:

```powershell
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

Для Docker Desktop Kubernetes з provider `kind` може знадобитися patch:

```powershell
$patchPath = Join-Path $env:TEMP 'metrics-server-strategic-patch.yaml'
@'
spec:
  template:
    spec:
      containers:
        - name: metrics-server
          args:
            - --cert-dir=/tmp
            - --secure-port=10250
            - --kubelet-preferred-address-types=InternalIP,ExternalIP,Hostname
            - --kubelet-use-node-status-port
            - --metric-resolution=15s
            - --kubelet-insecure-tls
'@ | Set-Content -LiteralPath $patchPath -Encoding UTF8

kubectl patch deployment metrics-server -n kube-system --type strategic --patch-file $patchPath
Remove-Item -LiteralPath $patchPath
kubectl rollout status deployment/metrics-server -n kube-system --timeout=180s
```

Під час навантаження дивитися, як HPA змінює кількість replicas:

```powershell
kubectl get hpa -w
```

## Навантажувальне тестування Locust

Запустити Locust у Kubernetes:

```powershell
kubectl apply -f k8s/locust.yaml
kubectl wait --for=condition=available deployment/medidispense-locust --timeout=120s
```

Відкрити UI:

```powershell
Start-Process http://localhost:8089
```

Якщо LoadBalancer для Locust не відкрився, використати port-forward:

```powershell
kubectl port-forward service/medidispense-locust 8089:8089
```

Налаштування в Locust UI:

- Host: `http://medidispense-api:8080`
- Users: `50`
- Spawn rate: `10`
- Run time: приблизно `2 minutes`

Провести однаковий тест для `replicas=1` і `replicas=3`.

Альтернатива, якщо Locust встановлений локально:

```powershell
locust -f load-tests/locustfile.py --host http://localhost:8080
```

Основний endpoint для демонстрації росту RPS:

```text
GET /api/diagnostics/work?iterations=750000
```

Цей endpoint виконує CPU-bound роботу. Якщо на комп'ютері достатньо CPU cores, при збільшенні кількості API pod-ів має зрости загальний RPS.

## Таблиця результатів

Заповнити під час тесту:

| API pods | Users | Spawn rate | Average RPS | Average latency | Failures |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 50 | 10 |  |  |  |
| 3 | 50 | 10 |  |  |  |

## Очистка після лабораторної

Видалити ресурси:

```powershell
kubectl delete -f k8s/locust.yaml
kubectl delete -f k8s/hpa.yaml
kubectl delete -f k8s/load-balancer.yaml
kubectl delete -f k8s/api.yaml
kubectl delete -f k8s/postgres.yaml
```

Видаляти PostgreSQL volume треба лише тоді, коли потрібно стерти дані бази:

```powershell
kubectl delete pvc medidispense-postgres-data
```
