example of a donation request:
10,14,0,3,33.8938,35.5018,2026-07-19 08:37:52.925286 +00:00,2026-07-21 12:00:00.000000 +00:00,0,1,"Beirut Medical Center, Beirut"


example of a donation post:

We have a seeker:
{
  "name": "Bob",
  "businessRole": 3,
  "latitude": 33.8938,
  "longitude": 35.5018,
  "isAvailable": true
}

We have a donor:
{
  "name": "Bib",
  "businessRole": 2,
  "latitude": 33.8938,
  "longitude": 35.5018,
  "isAvailable": true
}

# examples of donation posts:
{
  "bloodTypeName": 1,
  "quantity": 2
}

# examples of donation requests:
{
  "bloodTypeName": "O+",
  "quantity": 3,
  "latitude": 33.8938,
  "longitude": 35.5018,
  "urgency": 1,
  "neededByDate": "2026-07-21T12:00:00Z",
  "address": "Beirut Medical Center, Beirut"
}

{
  "bloodTypeName": "AB-",
  "quantity": 2,
  "latitude": 33.8547,
  "longitude": 35.8623,
  "urgency": 2,
  "neededByDate": "2026-07-24T09:00:00Z",
  "address": "Zahle Hospital, Zahle"
}
