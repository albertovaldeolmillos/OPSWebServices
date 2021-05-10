/* ################################################################################# */
/* ################################################################################# */
/* ################################################################################# */

create or replace trigger TR_AU_MAP_ALARMS
  after update on map_alarms
  for each row
declare
    nCount number;
begin

    SELECT COUNT(*) INTO nCount FROM map_alarms_his WHERE alxh_id = :new.alx_id;

    IF (nCount = 0) THEN

        INSERT INTO map_alarms_his
          ( alxh_id, alxh_timestamp_ini, alxh_timestamp_end
          , alxh_als_id, alxh_ald_id, alxh_all_id
          , alxh_gua_id, alxh_uni_id)
        VALUES
          ( :new.alx_id, :new.alx_timestamp, SYSDATE
          , :new.alx_als_id, :new.alx_ald_id, :new.alx_all_id
          , :new.alx_gua_id, :new.alx_uni_id);

    ELSE

        UPDATE map_alarms_his
        SET alxh_als_id = :new.alx_als_id, alxh_timestamp_end = SYSDATE
        WHERE alxh_id = :new.alx_id;

    END IF;

end TR_AU_MAP_ALARMS;
/

/* ################################################################################# */
/* ################################################################################# */
/* ################################################################################# */
