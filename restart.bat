:: Take psql user and password from user
@echo off
set /p username=Enter username for psql user: 
set /p password=Enter password for user %username%: 

:: Drop and recreate the 'yelpdb' database
set PGPASSWORD=%password%&& psql -U %username% -c "DROP DATABASE yelpdb"
set PGPASSWORD=%password%&& psql -U %username% -c "CREATE DATABASE yelpdb"
set PGPASSWORD=%password%&& psql -U %username% -c "GRANT ALL PRIVILEGES ON DATABASE yelpdb to %username%"

:: assumes this is being run from main project directory
set PGPASSWORD=%password%&& psql -U %username% -d yelpdb < DBSchema\procrastinators_RELATIONS_v2.sql

:: Run the python file to populate the database. psycopg2 will need to be installed
python DBPopulate\procrastinators_parseandinsert.py %username% %password%

:: Run update script and add triggers to database
set PGPASSWORD=%password%&& psql -U %username% -d yelpdb < SQLScripts/procrastinators_UPDATE.sql
set PGPASSWORD=%password%&& psql -U %username% -d yelpdb < SQLScripts/procrastinators_TRIGGER.sql