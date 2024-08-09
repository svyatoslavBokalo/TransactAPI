# TransactAPI
### In this project, I am going to write an import/export of data from excel to db, as well as work with transactions to db. In addition, work with different time zones.
#### So in this project, I had question that I faced.
This is in relation to time zones. Do I need to keep the data in the database or process everything in RAM. I decided that everything is better to keep in the database, because it is still easier than at first from the transaction location to calculate its time zone after working with this data. It will take a lot of time to process, it is better to do this once when importing data to the database.
I also used API to get time zones by location, for this it was necessary to register on the https://timezonedb.com/ and even then get the key, in principle as expected.
