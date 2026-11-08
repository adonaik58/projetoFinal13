# Kubernetes (preparação para migração)

Manifestos `kustomize` equivalentes ao `docker-compose.yml`, para quando a escala justificar Kubernetes.
A API já é *stateless*, expõe *liveness*/*readiness* e lê toda a configuração de variáveis de ambiente,
por isso não precisa de alterações de código.

```
kubectl apply -k infra/k8s            # depois de criar o Secret (ver secret.example.yaml)
```

* MySQL e Redis: recomenda-se serviços geridos (ou operadores como MySQL Operator / Redis Operator);
  ajuste `ConnectionStrings__*` no Secret.
* O `Ingress` substitui o Nginx de borda (TLS via cert-manager); o `HorizontalPodAutoscaler` substitui `API_REPLICAS`.
* As migrations correm no `Job` `parking-migrator` antes de cada rollout (`kubectl apply` cria um Job novo por versão).
