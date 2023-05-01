Clone the repositoty.
You can open VS and run it directly from there or follow the steps:

  1- Navigate to the DockerFile path in your commander.
  2- docker build -t theassignment -f Dockerfile .
  3- docker run -p 8080:80 -e ASPNETCORE_ENVIRONMENT=Development theassignment
  4- browse to http://localhost:8080/swagger/index.html

After swager page is open please try the post endpoint with request body e.g.:
[
  {
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "string",
    "deliveryDays": [
      "Tuesday", "Thursday", "Friday"
    ],
    "type": "Temporary",
    "daysInAdvance": 0
  },
{
    "productId": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "string",
    "deliveryDays": [
       "Tuesday", "Thursday"
    ],
    "type": "Normal",
    "daysInAdvance": 1
  }
]

The current greenDelivery is on Wednesday but in order to change please open appsetting and change the day in CronExpression.
Currently only day is supported as:
"? ? ? ? ? Wednesday"
or 
"? ? ? ? ? Wednesday,Friday"

Thanks!
