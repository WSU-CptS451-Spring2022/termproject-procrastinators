
-- When a tip is added, update the user's tipCount and the Businesses numTips counter
CREATE OR REPLACE FUNCTION tipUpdate() RETURNS TRIGGER AS 
$$
BEGIN
    UPDATE Usr SET tipCount = tipCount + 1 WHERE usr_id = NEW.usr_id;
    UPDATE Business SET numTips = numTips + 1 WHERE business_id = NEW.business_id;
    RETURN NEW;
END
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER tipAdded
AFTER INSERT ON Tip
FOR EACH ROW
EXECUTE PROCEDURE tipUpdate();


-- When a checkin is added, update the business's numCheckins counter
CREATE OR REPLACE FUNCTION checkinUpdate() RETURNS TRIGGER AS
$$
BEGIN
    UPDATE Business SET numCheckins = numCheckins + 1 WHERE business_id = NEW.business_id;
END
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER checkinAdded
AFTER INSERT ON CheckIns
FOR EACH ROW
EXECUTE PROCEDURE checkinUpdate();


-- When likes of a tip is changed, update the user's total likes
CREATE OR REPLACE FUNCTION likesUpdate() RETURNS TRIGGER AS
$$
BEGIN
    UPDATE Usr SET totalLikes = totalLikes + NEW.likes - OLD.likes WHERE usr_id = NEW.usr_id;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER tipLikesChanged
AFTER UPDATE ON Tip
FOR EACH ROW
EXECUTE PROCEDURE likesUpdate();