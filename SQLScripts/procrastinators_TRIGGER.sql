
-- a. Whenever a user provides a tip for a business, the "numTips" value for that business
--    and the "tipCount" value for the user should be updated

CREATE OR REPLACE FUNCTION iterate_vals() RETURNS trigger AS 
$$
BEGIN
    UPDATE Usr, Business
    SET tipCount = tipCount + 1, numTips = numTips + 1
    WHERE Usr.usr_id = NEW.usr_id AND Business.business_id = NEW.business_id
    RETURN NEW;
END;
$$
LANGUAGE 'plpgsql';

CREATE OR REPLACE FUNCTION iterate_numTips() RETURNS trigger AS
$$
BEGIN 
    UPDATE Business
    SET numTips = numTips + 1
    WHERE Business.business_id = NEW.business_id;
    RETURN NEW;
END;
$$ 
LANGUAGE 'plpgsql';

CREATE OR REPLACE FUNCTION iterate_tipCount() RETURNS trigger AS
$$
BEGIN
    UPDATE Usr
    SET tipCount = tipCount + 1
    WHERE Usr.usr_id = NEW.usr_id;
    RETURN NEW;
END;
$$
LANGUAGE 'plpgsql';

CREATE TRIGGER TipAdded
AFTER INSERT ON Tip
FOR EACH ROW 
EXECUTE PROCEDURE iterate_vals();
