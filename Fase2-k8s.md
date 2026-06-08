# Orquestração com Kubernetes — Oficina Mecânica API

## Visão Geral

Este documento descreve os manifestos Kubernetes para deploy da aplicação **Oficina Mecânica API**, uma aplicação ASP.NET Core 10 com banco de dados PostgreSQL 15. A orquestração contempla Deployments, Services, ConfigMaps, Secrets e Horizontal Pod Autoscaler (HPA).

---

## Estrutura dos Arquivos

```
k8s/
├── namespace.yaml
├── configmap.yaml
├── secret.yaml
├── postgres-deployment.yaml
├── postgres-service.yaml
├── api-deployment.yaml
├── api-service.yaml
└── hpa.yaml
```

---

## 1. Namespace

Isola todos os recursos da aplicação dentro do cluster.

```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: oficina-mecanica
```

---

## 2. ConfigMap

Armazena variáveis de configuração não sensíveis da aplicação.

```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: oficina-config
  namespace: oficina-mecanica
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ConnectionStrings__DefaultConnection: "Host=postgres-service;Port=5432;Database=OficinaDB;Username=postgres;Password=$(POSTGRES_PASSWORD);Trust Server Certificate=true"
  EmailSettings__SmtpServer: "smtp.seuprovedor.com"
  EmailSettings__Port: "587"
  EmailSettings__UseSSL: "true"
  EmailSettings__From: "noreply@oficinamecanica.com"
  Jwt__Issuer: "mecanica-api"
  Jwt__Audience: "mecanica-cliente"
  Jwt__ExpiracaoMinutos: "60"
```

> **Nota:** A string de conexão usa `$(POSTGRES_PASSWORD)` como referência à variável de ambiente injetada pelo Secret. No manifesto do Deployment, esse valor é montado via `env` diretamente.

---

## 3. Secret

Armazena variáveis sensíveis como credenciais e chaves criptográficas. Os valores devem ser codificados em **base64**.

```yaml
# k8s/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: oficina-secrets
  namespace: oficina-mecanica
type: Opaque
data:
  # echo -n "SuaSenhaSegura123!" | base64
  POSTGRES_PASSWORD: U3VhU2VuaGFTZWd1cmExMjMh
  # echo -n "sua-jwt-secret-key-minimo-32-caracteres!!" | base64
  JWT_SECRET_KEY: c3VhLWp3dC1zZWNyZXQta2V5LW1pbmltby0zMi1jYXJhY3RlcmVzISE=
  # echo -n "K7mP2nQx9vR4wL8sY1tZ6uA3cE5gJ0hF" | base64
  PASSWORD_KEY: SzdtUDJuUXg5dlI0d0w4c1kxdFo2dUEzY0U1Z0owaEY=
  # echo -n "usuario@email.com" | base64
  EMAIL_USERNAME: dXN1YXJpb0BlbWFpbC5jb20=
  # echo -n "SuaSenhaEmail" | base64
  EMAIL_PASSWORD: U3VhU2VuaGFFbWFpbA==
```

> **Importante:** Em produção, utilize um gerenciador de secrets como **AWS Secrets Manager**, **Azure Key Vault**, **HashiCorp Vault** ou **Sealed Secrets** para não versionar valores sensíveis no repositório.

---

## 4. PostgreSQL — Deployment

```yaml
# k8s/postgres-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: oficina-mecanica
  labels:
    app: postgres
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
        - name: postgres
          image: postgres:15
          ports:
            - containerPort: 5432
          env:
            - name: POSTGRES_DB
              value: "OficinaDB"
            - name: POSTGRES_USER
              value: "postgres"
            - name: POSTGRES_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: POSTGRES_PASSWORD
          resources:
            requests:
              cpu: "250m"
              memory: "256Mi"
            limits:
              cpu: "500m"
              memory: "512Mi"
          volumeMounts:
            - name: postgres-storage
              mountPath: /var/lib/postgresql/data
          livenessProbe:
            exec:
              command:
                - pg_isready
                - -U
                - postgres
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            exec:
              command:
                - pg_isready
                - -U
                - postgres
            initialDelaySeconds: 5
            periodSeconds: 5
      volumes:
        - name: postgres-storage
          persistentVolumeClaim:
            claimName: postgres-pvc
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-pvc
  namespace: oficina-mecanica
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 5Gi
```

---

## 5. PostgreSQL — Service

Expõe o banco de dados internamente no cluster (ClusterIP — não acessível externamente).

```yaml
# k8s/postgres-service.yaml
apiVersion: v1
kind: Service
metadata:
  name: postgres-service
  namespace: oficina-mecanica
spec:
  selector:
    app: postgres
  ports:
    - protocol: TCP
      port: 5432
      targetPort: 5432
  type: ClusterIP
```

---

## 6. API — Deployment

```yaml
# k8s/api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: oficina-api
  namespace: oficina-mecanica
  labels:
    app: oficina-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: oficina-api
  template:
    metadata:
      labels:
        app: oficina-api
    spec:
      containers:
        - name: oficina-api
          image: oficina_api:latest
          imagePullPolicy: IfNotPresent
          ports:
            - containerPort: 5000
          envFrom:
            - configMapRef:
                name: oficina-config
          env:
            - name: POSTGRES_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: POSTGRES_PASSWORD
            - name: Jwt__SecretKey
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: JWT_SECRET_KEY
            - name: Seguranca__PasswordKey
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: PASSWORD_KEY
            - name: EmailSettings__Username
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: EMAIL_USERNAME
            - name: EmailSettings__Password
              valueFrom:
                secretKeyRef:
                  name: oficina-secrets
                  key: EMAIL_PASSWORD
            - name: ConnectionStrings__DefaultConnection
              value: "Host=postgres-service;Port=5432;Database=OficinaDB;Username=postgres;Password=$(POSTGRES_PASSWORD);Trust Server Certificate=true"
          resources:
            requests:
              cpu: "250m"
              memory: "256Mi"
            limits:
              cpu: "1000m"
              memory: "512Mi"
          livenessProbe:
            httpGet:
              path: /health
              port: 5000
            initialDelaySeconds: 30
            periodSeconds: 15
            failureThreshold: 3
          readinessProbe:
            httpGet:
              path: /health
              port: 5000
            initialDelaySeconds: 10
            periodSeconds: 10
            failureThreshold: 3
```

> **Pré-requisito:** A imagem `oficina_api:latest` deve estar disponível em um registry acessível pelo cluster (Docker Hub, ECR, ACR, GCR, etc.). Para ambientes locais com Minikube, utilize `minikube image load oficina_api:latest`.

---

## 7. API — Service

Expõe a API internamente no cluster. Para acesso externo, utilize um **Ingress** ou altere o tipo para `LoadBalancer`.

```yaml
# k8s/api-service.yaml
apiVersion: v1
kind: Service
metadata:
  name: oficina-api-service
  namespace: oficina-mecanica
spec:
  selector:
    app: oficina-api
  ports:
    - protocol: TCP
      port: 80
      targetPort: 5000
  type: ClusterIP
```

### Opcional — Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: oficina-ingress
  namespace: oficina-mecanica
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
    - host: oficina.mecanica.local
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: oficina-api-service
                port:
                  number: 80
```

---

## 8. Horizontal Pod Autoscaler (HPA)

Escala automaticamente o número de réplicas da API com base no consumo de CPU e memória.

```yaml
# k8s/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: oficina-api-hpa
  namespace: oficina-mecanica
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: oficina-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
        - type: Pods
          value: 2
          periodSeconds: 60
    scaleDown:
      stabilizationWindowSeconds: 120
      policies:
        - type: Pods
          value: 1
          periodSeconds: 60
```

> **Pré-requisito:** O **Metrics Server** deve estar instalado no cluster para que o HPA funcione.
> ```bash
> kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
> ```

---

## Como Aplicar os Manifestos

### 1. Criar os recursos na ordem correta

```bash
# Criar o namespace
kubectl apply -f k8s/namespace.yaml

# Criar ConfigMap e Secrets
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml

# Subir o banco de dados
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/postgres-service.yaml

# Aguardar o postgres estar pronto
kubectl wait --for=condition=ready pod -l app=postgres -n oficina-mecanica --timeout=60s

# Subir a API
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml

# Configurar o HPA
kubectl apply -f k8s/hpa.yaml
```

### 2. Verificar o estado dos recursos

```bash
# Listar todos os recursos no namespace
kubectl get all -n oficina-mecanica

# Verificar os pods
kubectl get pods -n oficina-mecanica

# Verificar o HPA
kubectl get hpa -n oficina-mecanica

# Ver logs da API
kubectl logs -l app=oficina-api -n oficina-mecanica --follow
```

### 3. Acessar a API localmente (port-forward)

```bash
kubectl port-forward service/oficina-api-service 8080:80 -n oficina-mecanica
# API disponível em: http://localhost:8080
```

---

## Variáveis de Ambiente — Resumo

| Variável | Origem | Descrição |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap | Ambiente de execução |
| `ConnectionStrings__DefaultConnection` | ConfigMap + Secret | String de conexão com o PostgreSQL |
| `Jwt__SecretKey` | Secret | Chave para assinatura dos tokens JWT |
| `Jwt__Issuer` | ConfigMap | Emissor do token JWT |
| `Jwt__Audience` | ConfigMap | Audiência do token JWT |
| `Jwt__ExpiracaoMinutos` | ConfigMap | Tempo de expiração do token |
| `Seguranca__PasswordKey` | Secret | Chave para hash de senhas (Argon2) |
| `EmailSettings__SmtpServer` | ConfigMap | Servidor SMTP |
| `EmailSettings__Port` | ConfigMap | Porta SMTP |
| `EmailSettings__UseSSL` | ConfigMap | Habilitar SSL no SMTP |
| `EmailSettings__From` | ConfigMap | Endereço de origem dos e-mails |
| `EmailSettings__Username` | Secret | Usuário de autenticação SMTP |
| `EmailSettings__Password` | Secret | Senha de autenticação SMTP |
| `POSTGRES_PASSWORD` | Secret | Senha do banco de dados PostgreSQL |

---

## Considerações de Segurança

- Nunca versione o arquivo `secret.yaml` com valores reais no repositório. Utilize `.gitignore` ou ferramentas como **Sealed Secrets** / **External Secrets Operator**.
- Configure **RBAC** (Role-Based Access Control) para limitar quais pods têm acesso a quais Secrets.
- Utilize **Network Policies** para restringir a comunicação entre pods (ex: apenas a API pode acessar o PostgreSQL).
- Em produção, considere usar um **Ingress Controller** com TLS (cert-manager + Let's Encrypt).