#CptS 451 - Spring 2022
# https://www.psycopg.org/docs/usage.html#query-parameters

#  if psycopg2 is not installed, install it using pip installer :  pip install psycopg2  (or pip3 install psycopg2) 
import json
import psycopg2

def cleanStr4SQL(s):
    return s.replace("'","`").replace("\n"," ")

def int2BoolStr (value):
    if value == 0:
        return 'False'
    else:
        return 'True'

def unbox_dict(name, d):
    new_d = {}
    for k, v in d.items():
        if isinstance(v, dict): # This is never true so I really have no idea if it works
            t = unbox_dict(k, v)
            d = d | t           # Combines dictionaries
        else:
            new_d.update({name + "_" + k : v})
    return new_d

def insert2Business(conn):
    #reading the JSON file
    with open('DBPopulate/yelp_business.JSON','r') as f:
        line = f.readline()
        count_line = 0
        cur = conn.cursor()
        
        while line:
            data = json.loads(line)
            try:
                cur.execute("INSERT INTO Business (business_id, name, address, state, city, zipcode, latitude, longitude, stars, numCheckins, numTips, is_open)"
                       + " VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)", 
                         (data['business_id'],cleanStr4SQL(data["name"]), cleanStr4SQL(data["address"]), data["state"], data["city"], data["postal_code"], data["latitude"], data["longitude"], data["stars"], 0 , 0 , [False,True][data["is_open"]] ) )              
            except Exception as e:
                print("Insert to Business failed!",e)


            try: # Insert categories
                for cat in data['categories'].split(', '):
                    cur.execute(f"INSERT INTO Categories (business_id, category_name) VALUES ('{data['business_id']}', '{cleanStr4SQL(cat)}')")
            except Exception as e:
                print("Insert into Categories failed!", e)


            try: # Insert hours
                for k, v in data['hours'].items():
                    cur.execute("INSERT INTO Hrs (dayofweek, close, open, business_id) "
                            + f" VALUES ('{k}', '{v.split('-')[0]}', '{v.split('-')[1]}', '{data['business_id']}')")
            except Exception as e:
                print("Insert into Hrs failed!", e)

            d = {}
            try: # Insert attribues
                for k, v in data['attributes'].items():
                    if isinstance(v, dict):
                        d = unbox_dict(k, v)
                    else: 
                        d = {k:v}
                    for k2, v2 in d.items():
                        cur.execute("INSERT INTO Attributes (attr_name, val, business_id)"
                                + f" VALUES ('{cleanStr4SQL(k2)}', '{v2}', '{data['business_id']}')")
            except Exception as e:
                print("Insert into Attributes failed!", e)

            if count_line % 1000 == 0:
                print(count_line)

            conn.commit()

            line = f.readline()
            count_line +=1

        cur.close()

    print(f'FINISHED insert2Business - {count_line}')
    f.close()

def insert2Friend(conn, cur):
    with open('DBPopulate/yelp_user.JSON', 'r') as f:
        line = f.readline()
        count_line = 0

        while line:
            data = json.loads(line)

            try: # Insert Friends
                for fri in data['friends']:
                    fri.replace('"', "'")
                    if fri < data['user_id']:
                        cur.execute(f"INSERT INTO Friend (friend_of, friend_for) VALUES ('{data['user_id']}', '{fri}')")
            except Exception as e:
                print("Insert to Friend failed!", e)

            count_line += 1
            line = f.readline()
            conn.commit()

    f.close()
    


def insert2Usr(conn):
    with open('DBPopulate/yelp_user.JSON', 'r') as f:
        line = f.readline()
        count_line = 0

        cur = conn.cursor()

        while line:
            data = json.loads(line)

            try: # Insert Usr
                cur.execute("INSERT INTO Usr (average_stars, cool, fans, funny, name, tipCount, useful, usr_id, yelping_since, totalLikes)"
                        + " VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)", 
                        (data['average_stars'], data['cool'], data['fans'], data['funny'], cleanStr4SQL(data['name']), data['tipcount'], data['useful'], data['user_id'], data['yelping_since'], 0))
            except Exception as e:
                print("Insert to User failed!", e)
            
            conn.commit()

            line = f.readline()
            count_line += 1

        insert2Friend(conn, cur)
        print(f'FINISHED insert2Friend')
        cur.close()
    print(f'FINISHED insert2Usr - {count_line}')
    f.close()
    

def insert2Tip(conn):
    with open('DBPopulate/yelp_tip.JSON', 'r') as f:
        line = f.readline()
        count_line = 0

        cur = conn.cursor()

        while line:
            data = json.loads(line)
            
            try:
                cur.execute("INSERT INTO Tip (business_id, tipDate, likes, tipText, usr_id)"
                        + f" VALUES ('{data['business_id']}', '{data['date']}', {data['likes']}, '{cleanStr4SQL(data['text'])}', '{data['user_id']}')")
            except Exception as e:
                print("Insert to Tip failed!", e)
            conn.commit()

            line = f.readline()
            count_line += 1
    
        cur.close()
    
    print(f'FINISHED insert2Tip - {count_line}')
    f.close()

def insert2CheckIns(conn):
    with open('DBPopulate/yelp_checkin.JSON', 'r') as f:
        line = f.readline()
        count_line = 0

        cur = conn.cursor()

        while line:
            data = json.loads(line)
            
            try:
                dates = data['date'].split(',')
                for timestamp in dates:
                    date = timestamp.split(' ')[0]
                    time = timestamp.split(' ')[1]
                    
                    parts = date.split('-')
                    cur.execute("INSERT INTO CheckIns (ci_year, ci_month, ci_day, ci_time, business_id)" 
                            + f" VALUES ({parts[0]}, {parts[1]}, {parts[2]}, '{time}', '{data['business_id']}')")

            except Exception as e:
                print("Insert to CheckIns failed!", e)
            conn.commit()

            line = f.readline()
            count_line += 1

        cur.close()

    print(f'FINISHED insert2CheckIns - {count_line}')
    f.close()

if __name__ == "__main__":
    try: 
        conn = psycopg2.connect(dbname="yelpdb", host="localhost", user="postgres", password="12345")
        print('Starting insert2Business')
        insert2Business(conn)          # DONE
        print('Starting insert2Usr')
        insert2Usr(conn)               # DONE
        print('Starting insert2Tip')
        insert2Tip(conn)               # DONE
        print('starting insert2CheckIns')
        insert2CheckIns(conn)          # DONE
        conn.close()
    except Exception as e:
        print('Unable to connect to database', e)

    