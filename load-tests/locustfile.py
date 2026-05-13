from locust import HttpUser, between, task


class MediDispenseUser(HttpUser):
    wait_time = between(0.1, 0.5)
    token = None

    def on_start(self):
        response = self.client.post(
            "/api/auth/login",
            json={"username": "admin", "password": "admin"},
            name="/api/auth/login",
            catch_response=True,
        )

        if response.status_code != 200:
            response.failure(f"login failed with status {response.status_code}")
            return

        data = response.json()
        self.token = data.get("token") or data.get("Token")

        if not self.token:
            response.failure("login response did not contain a token")

    @property
    def auth_headers(self):
        if not self.token:
            return {}

        return {"Authorization": f"Bearer {self.token}"}

    @task(4)
    def synthetic_workload(self):
        self.client.get(
            "/api/diagnostics/work?iterations=750000",
            name="/api/diagnostics/work",
        )

    @task(2)
    def get_medications(self):
        self.client.get(
            "/api/medications",
            headers=self.auth_headers,
            name="/api/medications",
        )

    @task(1)
    def get_instance(self):
        self.client.get(
            "/api/diagnostics/instance",
            name="/api/diagnostics/instance",
        )
