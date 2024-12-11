## Databa setup 

Cd into /OrderManangement/OrderManagement


dotnet ef migrations add InitialCreate --context OrderStateDbContext

dotnet ef database update --context OrderStateDbContext

## Check container health status 

docker inspect --format='{{json .State.Health.Status}}' rabbitmq
docker inspect --format='{{json .State.Health.Status}}' mssql