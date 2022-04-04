
CREATE TABLE Usr (
    usr_id VARCHAR(30) PRIMARY KEY,
    name VARCHAR(100),
    average_stars FLOAT,
    fans INTEGER,
    cool INTEGER,
    tipCount INTEGER,
    funny INTEGER,
    totalLikes INTEGER,
    useful INTEGER,
    latitude FLOAT,
    longitude FLOAT,
    yelping_since TIMESTAMP WITHOUT TIME ZONE
);


CREATE TABLE Business (
    business_id     VARCHAR(30) PRIMARY KEY,
    name            VARCHAR(100),
    address         VARCHAR(100),
    state           VARCHAR(2),
    city            VARCHAR(40),
    zipcode         INTEGER,
    latitude        FLOAT,
    longitude       FLOAT,
    stars           FLOAT,
    numCheckins     INTEGER,
    numTips         INTEGER,
    is_open         BOOLEAN
);


CREATE TABLE Categories (
    category_name VARCHAR(100),
    business_id VARCHAR(30),
    PRIMARY KEY (business_id, category_name),
    FOREIGN KEY (business_id) REFERENCES Business (business_id)
);


CREATE TABLE Attributes (
    attr_name VARCHAR(50),
    val       VARCHAR(50),
    business_id VARCHAR(30),
    PRIMARY KEY (business_id, attr_name),
    FOREIGN KEY (business_id) REFERENCES Business (business_id)
);


CREATE TABLE Hrs (
    dayofweek VARCHAR(10),
    close TIME,
    open TIME,
    business_id VARCHAR(30),
    PRIMARY KEY (business_id, dayofweek),
    FOREIGN KEY (business_id) REFERENCES Business (business_id) 
);


CREATE TABLE CheckIns (
    ci_year    INTEGER,
    ci_month   INTEGER,
    ci_day     INTEGER,
    ci_time    TIME,
    business_id VARCHAR(30),
    PRIMARY KEY (ci_year, ci_month, ci_day, ci_time, business_id),
    FOREIGN KEY (business_id) REFERENCES Business (business_id)
);


CREATE TABLE Tip (
    tipDate TIMESTAMP,
    tipText VARCHAR(1000),
    likes INTEGER,
    usr_id VARCHAR(30),
    business_id VARCHAR(30),
    PRIMARY KEY (usr_id, business_id, tipDate),
    FOREIGN KEY (usr_id) REFERENCES Usr (usr_id),
    FOREIGN KEY (business_id) REFERENCES Business (business_id)
);


CREATE TABLE Friend (
    friend_for VARCHAR(30),
    friend_of  VARCHAR(30),
    PRIMARY KEY (friend_for, friend_of),
    FOREIGN KEY (friend_for) REFERENCES Usr (usr_id),
    FOREIGN KEY (friend_of)  REFERENCES Usr (usr_id)
);

