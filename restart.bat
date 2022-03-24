:: Drop and recreate the 'yelpdb' database
set PGPASSWORD=12345&& psql -U postgres -c "DROP DATABASE yelpdb"
set PGPASSWORD=12345&& psql -U postgres -c "CREATE DATABASE yelpdb"
set PGPASSWORD=12345&& psql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE yelpdb to postgres"

:: assumes this is being run from main project directory
set PGPASSWORD=12345&& psql -U postgres -d yelpdb < DBSchema\procrastinators_RELATIONS_v2.sql

:: Run the python file to populate the database. Should have a venv with psycopg2 installed
python DBPopulate\procrastinators_parseandinsert.py 