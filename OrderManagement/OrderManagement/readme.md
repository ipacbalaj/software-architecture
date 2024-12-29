## Databa setup 


## Create and run Migrations

### Prerequisites

`dotnet tool install --global dotnet-ef`

### Steps

Cd into /OrderManangement/OrderManagement


`dotnet ef migrations add InitialCreate --context OrderStateDbContext`

`dotnet ef database update --context OrderStateDbContext`

## Check container health status 

docker inspect --format='{{json .State.Health.Status}}' rabbitmq
docker inspect --format='{{json .State.Health.Status}}' mssql

## Kubernetes 

See all contexts 
`kubectl config get-contexts`

Switch to corect context
`kubectl config set-context docker-desktop`

`kubectl config use-context docker-desktop`

### Install Helm


### Setup Kubernetes dashboard locally

https://github.com/kubernetes/dashboard

https://gist.github.com/dahlsailrunner/1a47b0e38f6e3ba64d4d61835c73b7e2

### Install the dashboard 

kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.4.0/aio/deploy/recommended.yaml

### Create an admin user 

`kubectl apply -f admin-user.yaml`

### Create a token for accessing the dashboard

`kubectl -n kubernetes-dashboard create token admin-user`

### Proxy to the dashboard

`kubectl proxy`

### Navigate to 

http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/


## Kubernetes  Deployments
Cd into the kubernetes folder.

### Create deployment for SQL Server

`kubectl apply -f ./mssql-deployment.yml`

### Create deployment for Rabbit
`kubectl apply -f ./rabbit-deployment.yml`

### Create deployment for OrderManagement
`kubectl apply -f ./ordermanagement-deployment.yml`



### Remove deployment 

`kubectl delete -f ./ordermanagement-deployment.yml`

### Set image pull policy to make sure the images are pulled from local 

` imagePullPolicy: Never`

### Check the pods

`kubectl get pods`

### Check logs for a pod

`kubectl logs rabbitmq-df7dd796f-p4fx8`

### Check service configuration

`kubectl describe svc ordermanagement`