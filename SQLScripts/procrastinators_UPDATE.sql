

-- Update numCheckins
UPDATE Business 
SET numCheckins = t.n_cin 
FROM (SELECT COUNT(business_id) as n_cin, business_id as b_id
      FROM CheckIns
      GROUP BY business_id
    ) t
WHERE business_id = t.b_id;

-- Update numTips
UPDATE Business 
SET numTips = t.tip_c
FROM (SELECT COUNT(business_id) as tip_c, business_id as b_id
      FROM Tip
      GROUP BY business_id
    ) t
WHERE business_id = t.b_id;

-- Update tipCount
UPDATE Usr
SET tipCount = t_c 
FROM (SELECT COUNT(usr_id) as t_c, usr_id as u_id
      FROM Tip
      GROUP BY usr_id
    ) t
WHERE usr_id = u_id;

-- Update totalLikes
UPDATE Usr
SET totalLikes = t.like_sum 
FROM (SELECT SUM(likes) as like_sum, usr_id as u_id
      FROM Tip
      GROUP BY usr_id
    ) t
WHERE usr_id = u_id;