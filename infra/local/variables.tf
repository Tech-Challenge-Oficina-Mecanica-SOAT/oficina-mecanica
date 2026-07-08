variable "cluster_name" {
  description = "Nome do cluster Kind"
  type        = string
  default     = "oficina-mecanica"
}

variable "load_local_image" {
  description = "Se true, builda a imagem da API localmente com Docker e a carrega no Kind via 'kind load docker-image' (sem precisar de um registry). Use false em ambientes onde a imagem já foi publicada em um registry acessível pelo cluster."
  type        = bool
  default     = true
}

variable "api_image" {
  description = "Tag da imagem da API a ser usada no Deployment. Quando load_local_image=true, é a tag usada no 'docker build'/'kind load'. Quando false, deve ser uma imagem já publicada (ex: ghcr.io/usuario/oficina-mecanica-api:sha-abc123)."
  type        = string
  default     = "oficina-mecanica-api:local"
}

variable "bash_interpreter" {
  description = "Caminho para o bash. Default funciona em macOS/Linux. Windows (Git Bash): C:/Program Files/Git/bin/bash.exe"
  type        = string
  default     = "bash"
}
