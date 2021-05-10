/* ################################################################################################################### */
/* ### INICIO ######################################################################################################## */
/* ################################################################################################################### */

DECLARE
    nCount NUMBER;
    nRowID NUMBER;

BEGIN

/* ################################################################################################################### */
/* ### PARAMETERS #################################################################################################### */
/* ################################################################################################################### */

/* ATENCION: El siguiente parámetro de configuración permite establecer si los marcadores de los vigilantes */
/*           son dibujados con restricción a su zona. */

SELECT COUNT(*) INTO nCount FROM parameters WHERE par_descshort = 'P_MAPS_GUARD_ZONE';

IF (nCount = 0) THEN

    SELECT MAX(par_id) + 1 INTO nRowID FROM parameters;

    INSERT INTO parameters (par_id, par_descshort, par_desclong, par_value, par_category, par_version)
    VALUES (nRowID, 'P_MAPS_GUARD_ZONE', 'Mapas con vigilantes por zona (0: No, 1: Sí)', '0', 'CC', 0);

END IF;

/* ################################################################################################################### */
/* ### MAP_ZONES ##################################################################################################### */
/* ################################################################################################################### */

/* ATENCION: Las siguientes zonas NO son de prueba. Corresponden a los mapas de zonas de Zaragoza. */
/*           La configuración de zoom corresponde a una pantalla de 1280 x 1024. */

DELETE FROM map_zones WHERE zon_grp_id BETWEEN 50001 AND 50012;

INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50001, 41.65282, -0.8783 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50003, 41.65233, -0.889  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50004, 41.64999, -0.8842 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50005, 41.64911, -0.8794 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50006, 41.64459, -0.8823 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50007, 41.64457, -0.8884 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50008, 41.64687, -0.8899 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50009, 41.65005, -0.8924 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50010, 41.64478, -0.8967 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50011, 41.65108, -0.9021 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50012, 41.64695, -0.8993 ,17);

/* ################################################################################################################### */
/* ### FIN ########################################################################################################### */
/* ################################################################################################################### */

COMMIT WORK;

END;
/
/* ################################################################################################################### */
/* ################################################################################################################### */
/* ################################################################################################################### */
