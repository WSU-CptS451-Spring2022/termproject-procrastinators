# termproject-procrastinators
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)

## Setup Instructions
NOTE : The commands in this file and scripts in the repository only work on Windows. The program should still run on UNIX systems, but these instructions will not be sufficient.
### PostgreSQL
A local installation of PostgreSQL is required for this program to function properly. If you do not have PostgreSQL installed, please head [here](https://www.postgresql.org/download/) and find the download appropriate for your system. 

### Python
If you do not have > Python3.9 installed, please visit [here](https://www.python.org/downloads/) to download the appropriate version of Python. If you are not sure which version of python you have, use `python --version` to find out.

It is highly recommended that you run the program in a python virtual environment (venv). To set up a venv, type
```
python -m venv venv
```
You can activate your venv by entering the following:
```
.\venv\Scripts\activate
```
Once inside your venv, you can download all dependencies of this project with the following.
```
pip install -r requirements.txt
```

### Scripts
To drop the yelpdb database, create a new one, and repopulate it, enter the following. The script will then prompt for you for the name and password of the user you wish to log into psql with. If yelpdb already exists, the provided user will need to have permission to DROP the existing yelpdb. 
```
.\restart.bat
```
Please note, for the DB to populate correctly, you will need the appropriate JSON files to be located in the DBPopulate folder.

This process may take a while, as there is a lot of data to insert. The program should print start and finish statements for each JSON file it reads in.
