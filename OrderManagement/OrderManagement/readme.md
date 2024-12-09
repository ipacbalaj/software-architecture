## Databa setup 

Cd into /OrderManangement/OrderManagement


dotnet ef migrations add InitialCreate --context OrderStateDbContext

dotnet ef database update --context OrderStateDbContext

