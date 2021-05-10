/* #################################################################################################################################################################### */
/* ### INICIO ######################################################################################################################################################### */
/* #################################################################################################################################################################### */

DECLARE
    nCount  NUMBER;
    nViewID NUMBER;
	nRowID  NUMBER;

BEGIN

/* #################################################################################################################################################################### */
/* ### PARAMETERS ##################################################################################################################################################### */
/* #################################################################################################################################################################### */

/* ATENCION: El siguiente parámetro de configuración permite activar/desactivar los diferentes modos de mapas. */
/*           Notar que es posible activar todos a la vez. */

SELECT COUNT(*) INTO nCount FROM parameters WHERE par_descshort = 'P_MAPS_ACTIVE';

IF (nCount = 0) THEN

	SELECT MAX(par_id) + 1 INTO nRowID FROM parameters;

	INSERT INTO parameters (par_id, par_descshort, par_desclong, par_value, par_category, par_version)
	VALUES (nRowID, 'P_MAPS_ACTIVE', 'Mapas activos (0: Todos, 1: Imagen, 2: Google)', '2', 'CC', 0);

END IF;

/* ATENCION: El siguiente parámetro de configuración permite establecer si los marcadores de los vigilantes */
/*           son dibujados con restricción a su zona. */

SELECT COUNT(*) INTO nCount FROM parameters WHERE par_descshort = 'P_MAPS_GUARD_ZONE';

IF (nCount = 0) THEN

    SELECT MAX(par_id) + 1 INTO nRowID FROM parameters;

    INSERT INTO parameters (par_id, par_descshort, par_desclong, par_value, par_category, par_version)
    VALUES (nRowID, 'P_MAPS_GUARD_ZONE', 'Mapas con vigilantes por zona (0: No, 1: Sí)', '0', 'CC', 0);

END IF;

/* #################################################################################################################################################################### */
/* ### VIEWS, VIEWS_ELEMENTS, USR_ACCESS y USR_PERMISSIONS ############################################################################################################ */
/* #################################################################################################################################################################### */

SELECT COUNT(*) INTO nCount FROM views WHERE vie_url = 'opsmap.aspx';

IF (nCount = 0) THEN

	SELECT MAX(vie_id) + 1 INTO nViewID FROM views;
    INSERT INTO views (vie_id, vie_url, vie_lit_id, vie_title_lit_id, vie_version)
    VALUES (nViewID, 'opsmap.aspx', 1010, 1010, 1);

	SELECT MAX(vele_id) + 1 INTO nRowID FROM views_elements;
	INSERT INTO views_elements (vele_id, vele_vie_id, vele_elementnumber, vele_descshort, vele_desclong, vele_version)
	VALUES (nRowID, nViewID, 1, 'opsmap.aspx', 'opsmap.aspx', 1);

	/* ATENCION: La siguiente configuración de acceso corresponde a roles/usuarios de Zaragoza.                    */
	/*           Los roles que pueden acceder a "WFCallejero.aspx" (id=10) también pueden acceder a "opsmap.aspx". */
	/*           Sólo el rol "Administrador" (id=1) y el usuario "Admin" (id=1) tienen permisos de actualización.  */

	INSERT INTO rol_access (racc_rol_id, racc_vie_id, racc_allowed, racc_version)
		SELECT racc_rol_id, nViewID, 1, 1
		FROM rol_access
		WHERE (racc_vie_id = 10) AND (racc_allowed = 1) AND (racc_valid = 1) AND (racc_deleted = 0);

	/* Un trigger de rol_access genera los siguientes registros, que a continuación son reconfigurados: */

	UPDATE rol_permissions
	SET rper_insallowed = 1, rper_updallowed = 1, rper_delallowed = 1, rper_exeallowed = 1
	WHERE (rper_vele_vie_id = nViewID) AND (rper_vele_elementnumber = 1) AND (rper_rol_id = 1);

	UPDATE rol_permissions
	SET rper_insallowed = 0, rper_updallowed = 0, rper_delallowed = 0, rper_exeallowed = 1
	WHERE (rper_vele_vie_id = nViewID) AND (rper_vele_elementnumber = 1) AND (rper_rol_id <> 1)
	  AND (rper_rol_id IN (
		SELECT racc_rol_id FROM rol_access
		WHERE (racc_vie_id = 10) AND (racc_allowed = 1) AND (racc_valid = 1) AND (racc_deleted = 0)
	  ));

	UPDATE usr_access
	SET uacc_allowed = 1
	WHERE uacc_vie_id = nViewID;

	UPDATE usr_permissions
	SET uper_insallowed = 1, uper_updallowed = 1, uper_delallowed = 1, uper_exeallowed = 1
	WHERE (uper_vele_vie_id = nViewID) AND (uper_usr_id = 1);

	UPDATE usr_permissions
	SET uper_insallowed = 0, uper_updallowed = 0, uper_delallowed = 0, uper_exeallowed = 1
	WHERE (uper_vele_vie_id = nViewID) AND (uper_usr_id <> 1);

END IF;

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_DEF ################################################################################################################################################# */
/* #################################################################################################################################################################### */

/* ATENCION: No alterar los identificadores, ya que están siendo referenciados por código fuente. */

SELECT COUNT(*) INTO nCount FROM map_alarms_def;

IF (nCount = 0) THEN

	INSERT INTO map_alarms_def (ald_id, ald_name) VALUES (1, 'Conectividad');
	INSERT INTO map_alarms_def (ald_id, ald_name) VALUES (2, 'Mobilidad');
	INSERT INTO map_alarms_def (ald_id, ald_name) VALUES (3, 'Proximidad');

END IF;

UPDATE map_alarms_def
SET ald_name_lit_id = (SELECT lit_id
                       FROM literals
                       WHERE (lit_lan_id = 1)
                       AND (lit_descshort = ('MAP_ALARMS_DEF.ALD_NAME.' || ald_id)));

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_STATUS ############################################################################################################################################## */
/* #################################################################################################################################################################### */

/* ATENCION: No alterar los identificadores, ya que están siendo referenciados por código fuente. */

SELECT COUNT(*) INTO nCount FROM map_alarms_status;

IF (nCount = 0) THEN

	INSERT INTO map_alarms_status (als_id, als_name) VALUES (0, 'Pendiente');
	INSERT INTO map_alarms_status (als_id, als_name) VALUES (1, 'Confirmada');

END IF;

UPDATE map_alarms_status
SET als_name_lit_id = (SELECT lit_id
                       FROM literals
                       WHERE (lit_lan_id = 1)
                       AND (lit_descshort = ('MAP_ALARMS_STATUS.ALS_NAME.' || als_id)));

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_LEVEL ############################################################################################################################################### */
/* #################################################################################################################################################################### */

/* ATENCION: No alterar los identificadores, ya que están siendo referenciados por código fuente. */

SELECT COUNT(*) INTO nCount FROM map_alarms_level;

IF (nCount = 0) THEN

	INSERT INTO map_alarms_level (all_id, all_name) VALUES (1, 'Alerta');
	INSERT INTO map_alarms_level (all_id, all_name) VALUES (2, 'Advertencia');

END IF;

UPDATE map_alarms_level
SET all_name_lit_id = (SELECT lit_id
                       FROM literals
                       WHERE (lit_lan_id = 1)
                       AND (lit_descshort = ('MAP_ALARMS_LEVEL.ALL_NAME.' || all_id)));

/* #################################################################################################################################################################### */
/* ### MAP_ZONES ###################################################################################################################################################### */
/* #################################################################################################################################################################### */

/* ATENCION: Las siguientes zonas NO son de prueba. Corresponden a los mapas de zonas de Zaragoza. */
/*           La configuración de zoom corresponde a una pantalla de 1280 x 1024. */

DELETE FROM map_zones;

INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50001, 41.65282, -0.8783  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50003, 41.65233, -0.889   ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50004, 41.64999, -0.8842  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50005, 41.64911, -0.8794  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50006, 41.64459, -0.8823  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50007, 41.64457, -0.8884  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50008, 41.64687, -0.8899  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50009, 41.65005, -0.8924  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50010, 41.64478, -0.8967  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50011, 41.65108, -0.9021  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (50012, 41.64695, -0.8993  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60001, 41.65264, -0.8778  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60002, 41.65383, -0.87669 ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60005, 41.65255, -0.8891  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60006, 41.6528 , -0.8888  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60007, 41.65111, -0.8846  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60008, 41.649  , -0.886   ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60009, 41.64906, -0.8798  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60010, 41.6492 , -0.8787  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60011, 41.6447 , -0.8824  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60012, 41.6445 , -0.8824  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60013, 41.64457, -0.8884  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60014, 41.64377, -0.8881  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60015, 41.64676, -0.89    ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60016, 41.64687, -0.8899  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60017, 41.64948, -0.8914  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60018, 41.65101, -0.8924  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60019, 41.6446 , -0.8956  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60020, 41.64478, -0.8967  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60021, 41.65108, -0.9021  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60022, 41.65112, -0.9021  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60023, 41.64695, -0.8993  ,17);
INSERT INTO map_zones (zon_grp_id, zon_lat, zon_lon, zon_zoom) VALUES (60024, 41.64759, -0.89619 ,17);

/* #################################################################################################################################################################### */
/* ### MAP_PKMETERS ################################################################################################################################################### */
/* #################################################################################################################################################################### */

/* ATENCION: Los siguientes parquímetros NO son de prueba. Corresponden a los parquímetros de Zaragoza.   */
/*           Es importante recordar que las conversiones de coordenadas son por grupo y aproximadas.      */
/*           A continuación de las conversiones se encuentran las coordenadas por posicionamiento manual. */

/* #################################################################################################################################################################### */

DELETE FROM map_pkmeters;

INSERT INTO map_pkmeters (pkm_uni_id, pkm_grp_id, pkm_lat, pkm_lon)
SELECT t2.uni_id, t1.cgrpg_id, NULL, NULL
FROM groups_childs_gis t1
INNER JOIN units t2         ON t2.uni_id   = t1.cgrpg_child
INNER JOIN units_phy_def t3 ON t3.dpuni_id = t2.uni_dpuni_id
WHERE (t1.cgrpg_deleted = 0)
  AND (t1.cgrpg_valid  != 0)
  AND (t1.cgrpg_type    = 'U')
  AND (t2.uni_deleted   = 0)
  AND (t2.uni_valid    != 0)
  AND (t3.dpuni_id IN (1,2))
  AND (t3.dpuni_deleted = 0)
  AND (t3.dpuni_valid  != 0);

/* #################################################################################################################################################################### */

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60001)
SET pkm_lat = 41.65620 - (uni_posx / 100000)  -- Abajo: - / Arriba: +
  , pkm_lon = -0.88065 + (uni_posy / 100000); -- Der..: - / Izq ..: +

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60002)
SET pkm_lat = 41.65480 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88120 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60005)
SET pkm_lat = 41.65641 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89341 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60006)
SET pkm_lat = 41.65641 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89331 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60007)
SET pkm_lat = 41.65400 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88758 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60008)
SET pkm_lat = 41.65468 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88786 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60009)
SET pkm_lat = 41.65333 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88475 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60010)
SET pkm_lat = 41.65285 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88485 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60011)
SET pkm_lat = 41.64868 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88608 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60012)
SET pkm_lat = 41.64868 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.88608 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60013)
SET pkm_lat = 41.64802 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89188 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60014)
SET pkm_lat = 41.64843 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89188 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60015)
SET pkm_lat = 41.65051 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89367 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60016)
SET pkm_lat = 41.65051 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89367 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60017)
SET pkm_lat = 41.65395 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89445 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60018)
SET pkm_lat = 41.65255 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89445 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60019)
SET pkm_lat = 41.64792 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89901 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60020)
SET pkm_lat = 41.64792 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.89916 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60021)
SET pkm_lat = 41.65441 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.90517 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60022)
SET pkm_lat = 41.65441 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.90517 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60023)
SET pkm_lat = 41.64922 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.90217 + (uni_posy / 100000); -- Izq...: + / Der..: -

UPDATE (SELECT * FROM map_pkmeters, units WHERE pkm_uni_id = uni_id AND pkm_grp_id = 60024)
SET pkm_lat = 41.64963 - (uni_posx / 100000)  -- Arriba: + / Abajo: -
  , pkm_lon = -0.90217 + (uni_posy / 100000); -- Izq...: + / Der..: -

/* #################################################################################################################################################################### */

UPDATE map_pkmeters SET pkm_lat = 41.652278291894, pkm_lon = -0.87910564389541 WHERE pkm_uni_id = 10101;
UPDATE map_pkmeters SET pkm_lat = 41.651965647619, pkm_lon = -0.87819905724841 WHERE pkm_uni_id = 10102;
UPDATE map_pkmeters SET pkm_lat = 41.651376429284, pkm_lon = -0.87775917496999 WHERE pkm_uni_id = 10103;
UPDATE map_pkmeters SET pkm_lat = 41.651829366306, pkm_lon = -0.87726028409324 WHERE pkm_uni_id = 10104;
UPDATE map_pkmeters SET pkm_lat = 41.652811386965, pkm_lon = -0.87825270142870 WHERE pkm_uni_id = 10105;
UPDATE map_pkmeters SET pkm_lat = 41.652198126839, pkm_lon = -0.87679357972467 WHERE pkm_uni_id = 10106;
UPDATE map_pkmeters SET pkm_lat = 41.651741184126, pkm_lon = -0.87573678937284 WHERE pkm_uni_id = 10107;
UPDATE map_pkmeters SET pkm_lat = 41.653336028628, pkm_lon = -0.87585524110164 WHERE pkm_uni_id = 10201;
UPDATE map_pkmeters SET pkm_lat = 41.653780935560, pkm_lon = -0.87670818356837 WHERE pkm_uni_id = 10202;
UPDATE map_pkmeters SET pkm_lat = 41.653317991798, pkm_lon = -0.87903634099321 WHERE pkm_uni_id = 10203;
UPDATE map_pkmeters SET pkm_lat = 41.653835663461, pkm_lon = -0.89147852750149 WHERE pkm_uni_id = 30101;
UPDATE map_pkmeters SET pkm_lat = 41.655018057830, pkm_lon = -0.89155899377194 WHERE pkm_uni_id = 30102;
UPDATE map_pkmeters SET pkm_lat = 41.654593199979, pkm_lon = -0.89052366109223 WHERE pkm_uni_id = 30103;
UPDATE map_pkmeters SET pkm_lat = 41.654669353951, pkm_lon = -0.88950978608464 WHERE pkm_uni_id = 30104;
UPDATE map_pkmeters SET pkm_lat = 41.652136184046, pkm_lon = -0.88669346661895 WHERE pkm_uni_id = 30105;
UPDATE map_pkmeters SET pkm_lat = 41.652673288516, pkm_lon = -0.88625894875850 WHERE pkm_uni_id = 30106;
UPDATE map_pkmeters SET pkm_lat = 41.652989937562, pkm_lon = -0.88476227612825 WHERE pkm_uni_id = 30107;
UPDATE map_pkmeters SET pkm_lat = 41.652352629668, pkm_lon = -0.88518606515258 WHERE pkm_uni_id = 30108;
UPDATE map_pkmeters SET pkm_lat = 41.650909645100, pkm_lon = -0.88749276490535 WHERE pkm_uni_id = 30109;
UPDATE map_pkmeters SET pkm_lat = 41.651585046068, pkm_lon = -0.88881777615883 WHERE pkm_uni_id = 30110;
UPDATE map_pkmeters SET pkm_lat = 41.650420626328, pkm_lon = -0.88779317231500 WHERE pkm_uni_id = 30111;
UPDATE map_pkmeters SET pkm_lat = 41.650757328176, pkm_lon = -0.88854419083915 WHERE pkm_uni_id = 30112;
UPDATE map_pkmeters SET pkm_lat = 41.653306070961, pkm_lon = -0.88960675301869 WHERE pkm_uni_id = 30201;
UPDATE map_pkmeters SET pkm_lat = 41.652626679809, pkm_lon = -0.88824419083918 WHERE pkm_uni_id = 30202;
UPDATE map_pkmeters SET pkm_lat = 41.651494345297, pkm_lon = -0.88975159230550 WHERE pkm_uni_id = 30203;
UPDATE map_pkmeters SET pkm_lat = 41.652330070215, pkm_lon = -0.88901130261741 WHERE pkm_uni_id = 30204;
UPDATE map_pkmeters SET pkm_lat = 41.649886483275, pkm_lon = -0.88418903012595 WHERE pkm_uni_id = 40101;
UPDATE map_pkmeters SET pkm_lat = 41.649688067216, pkm_lon = -0.88541211743671 WHERE pkm_uni_id = 40102;
UPDATE map_pkmeters SET pkm_lat = 41.651072958461, pkm_lon = -0.88559987206774 WHERE pkm_uni_id = 40103;
UPDATE map_pkmeters SET pkm_lat = 41.651766394990, pkm_lon = -0.88438751359305 WHERE pkm_uni_id = 40104;
UPDATE map_pkmeters SET pkm_lat = 41.652934922604, pkm_lon = -0.88393209031426 WHERE pkm_uni_id = 40105;
UPDATE map_pkmeters SET pkm_lat = 41.652375650982, pkm_lon = -0.88439287801110 WHERE pkm_uni_id = 40106;
UPDATE map_pkmeters SET pkm_lat = 41.652010899731, pkm_lon = -0.88497223515828 WHERE pkm_uni_id = 40107;
UPDATE map_pkmeters SET pkm_lat = 41.649449982872, pkm_lon = -0.88701596532185 WHERE pkm_uni_id = 40201;
UPDATE map_pkmeters SET pkm_lat = 41.648658313835, pkm_lon = -0.88535299573267 WHERE pkm_uni_id = 40202;
UPDATE map_pkmeters SET pkm_lat = 41.649088806930, pkm_lon = -0.88603438992812 WHERE pkm_uni_id = 40301;
UPDATE map_pkmeters SET pkm_lat = 41.648650170096, pkm_lon = -0.88267205396007 WHERE pkm_uni_id = 50101;
UPDATE map_pkmeters SET pkm_lat = 41.649375700158, pkm_lon = -0.88278470673868 WHERE pkm_uni_id = 50102;
UPDATE map_pkmeters SET pkm_lat = 41.648521898738, pkm_lon = -0.88098762669878 WHERE pkm_uni_id = 50103;
UPDATE map_pkmeters SET pkm_lat = 41.649339624271, pkm_lon = -0.88049410024006 WHERE pkm_uni_id = 50104;
UPDATE map_pkmeters SET pkm_lat = 41.649632239220, pkm_lon = -0.88138459363297 WHERE pkm_uni_id = 50105;
UPDATE map_pkmeters SET pkm_lat = 41.650317674514, pkm_lon = -0.88169036546066 WHERE pkm_uni_id = 50106;
UPDATE map_pkmeters SET pkm_lat = 41.649944894820, pkm_lon = -0.88012931981405 WHERE pkm_uni_id = 50107;
UPDATE map_pkmeters SET pkm_lat = 41.649010931922, pkm_lon = -0.88176010289503 WHERE pkm_uni_id = 50108;
UPDATE map_pkmeters SET pkm_lat = 41.648930762801, pkm_lon = -0.87696431317658 WHERE pkm_uni_id = 50109;
UPDATE map_pkmeters SET pkm_lat = 41.647880538114, pkm_lon = -0.87773142495480 WHERE pkm_uni_id = 50110;
UPDATE map_pkmeters SET pkm_lat = 41.647287273853, pkm_lon = -0.87885795274101 WHERE pkm_uni_id = 50111;
UPDATE map_pkmeters SET pkm_lat = 41.649371396116, pkm_lon = -0.87730197720848 WHERE pkm_uni_id = 50201;
UPDATE map_pkmeters SET pkm_lat = 41.649014644762, pkm_lon = -0.87795643620808 WHERE pkm_uni_id = 50202;
UPDATE map_pkmeters SET pkm_lat = 41.649635952024, pkm_lon = -0.87809054665883 WHERE pkm_uni_id = 50203;
UPDATE map_pkmeters SET pkm_lat = 41.650403558852, pkm_lon = -0.87923853211721 WHERE pkm_uni_id = 50204;
UPDATE map_pkmeters SET pkm_lat = 41.649467598391, pkm_lon = -0.87943165116625 WHERE pkm_uni_id = 50205;
UPDATE map_pkmeters SET pkm_lat = 41.648473501297, pkm_lon = -0.88000027947739 WHERE pkm_uni_id = 50206;
UPDATE map_pkmeters SET pkm_lat = 41.648220966123, pkm_lon = -0.88019339852641 WHERE pkm_uni_id = 50207;
UPDATE map_pkmeters SET pkm_lat = 41.648645866004, pkm_lon = -0.87878255658464 WHERE pkm_uni_id = 50208;
UPDATE map_pkmeters SET pkm_lat = 41.649551775262, pkm_lon = -0.87891666703538 WHERE pkm_uni_id = 50209;
UPDATE map_pkmeters SET pkm_lat = 41.648401348490, pkm_lon = -0.87778477483114 WHERE pkm_uni_id = 50210;
UPDATE map_pkmeters SET pkm_lat = 41.641539865099, pkm_lon = -0.88458590543438 WHERE pkm_uni_id = 60101;
UPDATE map_pkmeters SET pkm_lat = 41.642501998316, pkm_lon = -0.88562123811408 WHERE pkm_uni_id = 60102;
UPDATE map_pkmeters SET pkm_lat = 41.642381732450, pkm_lon = -0.88429622686078 WHERE pkm_uni_id = 60103;
UPDATE map_pkmeters SET pkm_lat = 41.643019138983, pkm_lon = -0.88325552976300 WHERE pkm_uni_id = 60104;
UPDATE map_pkmeters SET pkm_lat = 41.643087288994, pkm_lon = -0.88419966733620 WHERE pkm_uni_id = 60105;
UPDATE map_pkmeters SET pkm_lat = 41.645556676065, pkm_lon = -0.88450007474581 WHERE pkm_uni_id = 60106;
UPDATE map_pkmeters SET pkm_lat = 41.644582562272, pkm_lon = -0.88384025132817 WHERE pkm_uni_id = 60107;
UPDATE map_pkmeters SET pkm_lat = 41.644394151732, pkm_lon = -0.88334136045142 WHERE pkm_uni_id = 60108;
UPDATE map_pkmeters SET pkm_lat = 41.643644512852, pkm_lon = -0.88306241071394 WHERE pkm_uni_id = 60109;
UPDATE map_pkmeters SET pkm_lat = 41.642963015390, pkm_lon = -0.88262252843551 WHERE pkm_uni_id = 60110;
UPDATE map_pkmeters SET pkm_lat = 41.646539016293, pkm_lon = -0.88367335500155 WHERE pkm_uni_id = 60111;
UPDATE map_pkmeters SET pkm_lat = 41.646158197473, pkm_lon = -0.88300280274785 WHERE pkm_uni_id = 60112;
UPDATE map_pkmeters SET pkm_lat = 41.645027753612, pkm_lon = -0.88228397073189 WHERE pkm_uni_id = 60113;
UPDATE map_pkmeters SET pkm_lat = 41.644386358206, pkm_lon = -0.88161878289622 WHERE pkm_uni_id = 60114;
UPDATE map_pkmeters SET pkm_lat = 41.643957197508, pkm_lon = -0.88101320302659 WHERE pkm_uni_id = 60115;
UPDATE map_pkmeters SET pkm_lat = 41.647555190231, pkm_lon = -0.88260047139563 WHERE pkm_uni_id = 60116;
UPDATE map_pkmeters SET pkm_lat = 41.647237512332, pkm_lon = -0.88319055737895 WHERE pkm_uni_id = 60117;
UPDATE map_pkmeters SET pkm_lat = 41.647029066674, pkm_lon = -0.88265947999402 WHERE pkm_uni_id = 60118;
UPDATE map_pkmeters SET pkm_lat = 41.646873734132, pkm_lon = -0.88144712151928 WHERE pkm_uni_id = 60119;
UPDATE map_pkmeters SET pkm_lat = 41.644978561684, pkm_lon = -0.88349761491225 WHERE pkm_uni_id = 60120;
UPDATE map_pkmeters SET pkm_lat = 41.645600995353, pkm_lon = -0.88277749719061 WHERE pkm_uni_id = 60121;
UPDATE map_pkmeters SET pkm_lat = 41.646356624400, pkm_lon = -0.88138811292095 WHERE pkm_uni_id = 60122;
UPDATE map_pkmeters SET pkm_lat = 41.646865716959, pkm_lon = -0.88073901833938 WHERE pkm_uni_id = 60123;
UPDATE map_pkmeters SET pkm_lat = 41.645727268196, pkm_lon = -0.87912969293051 WHERE pkm_uni_id = 60124;
UPDATE map_pkmeters SET pkm_lat = 41.645298340381, pkm_lon = -0.87988071145462 WHERE pkm_uni_id = 60125;
UPDATE map_pkmeters SET pkm_lat = 41.644957601297, pkm_lon = -0.88055126370832 WHERE pkm_uni_id = 60126;
UPDATE map_pkmeters SET pkm_lat = 41.644761174418, pkm_lon = -0.88135056199472 WHERE pkm_uni_id = 60127;
UPDATE map_pkmeters SET pkm_lat = 41.644444484924, pkm_lon = -0.88201038541236 WHERE pkm_uni_id = 60128;
UPDATE map_pkmeters SET pkm_lat = 41.643941162434, pkm_lon = -0.88259570634530 WHERE pkm_uni_id = 60129;
UPDATE map_pkmeters SET pkm_lat = 41.646585115258, pkm_lon = -0.87951056661061 WHERE pkm_uni_id = 60130;
UPDATE map_pkmeters SET pkm_lat = 41.645434635512, pkm_lon = -0.87845914067681 WHERE pkm_uni_id = 60131;
UPDATE map_pkmeters SET pkm_lat = 41.644277898145, pkm_lon = -0.87893717324914 WHERE pkm_uni_id = 60132;
UPDATE map_pkmeters SET pkm_lat = 41.643584381011, pkm_lon = -0.88032119310077 WHERE pkm_uni_id = 60133;
UPDATE map_pkmeters SET pkm_lat = 41.643243632865, pkm_lon = -0.88108294046102 WHERE pkm_uni_id = 60134;
UPDATE map_pkmeters SET pkm_lat = 41.641788417557, pkm_lon = -0.88311069047624 WHERE pkm_uni_id = 60135;
UPDATE map_pkmeters SET pkm_lat = 41.641595989932, pkm_lon = -0.88281028306659 WHERE pkm_uni_id = 60136;
UPDATE map_pkmeters SET pkm_lat = 41.643115350740, pkm_lon = -0.88068060910875 WHERE pkm_uni_id = 60137;
UPDATE map_pkmeters SET pkm_lat = 41.643628477698, pkm_lon = -0.87955944574057 WHERE pkm_uni_id = 60138;
UPDATE map_pkmeters SET pkm_lat = 41.642249439738, pkm_lon = -0.88333599603348 WHERE pkm_uni_id = 60301;
UPDATE map_pkmeters SET pkm_lat = 41.641736961866, pkm_lon = -0.88400058450142 WHERE pkm_uni_id = 60201;
UPDATE map_pkmeters SET pkm_lat = 41.642089744635, pkm_lon = -0.88470332326330 WHERE pkm_uni_id = 60202;
UPDATE map_pkmeters SET pkm_lat = 41.642747198277, pkm_lon = -0.88457457723059 WHERE pkm_uni_id = 60203;
UPDATE map_pkmeters SET pkm_lat = 41.643382596795, pkm_lon = -0.88148467244556 WHERE pkm_uni_id = 60204;
UPDATE map_pkmeters SET pkm_lat = 41.643939818099, pkm_lon = -0.88220886887955 WHERE pkm_uni_id = 60205;
UPDATE map_pkmeters SET pkm_lat = 41.644444921016, pkm_lon = -0.88240198792862 WHERE pkm_uni_id = 60206;
UPDATE map_pkmeters SET pkm_lat = 41.644966054797, pkm_lon = -0.88321737946910 WHERE pkm_uni_id = 60207;
UPDATE map_pkmeters SET pkm_lat = 41.645403002180, pkm_lon = -0.88365726174753 WHERE pkm_uni_id = 60208;
UPDATE map_pkmeters SET pkm_lat = 41.646136586074, pkm_lon = -0.88405422868168 WHERE pkm_uni_id = 60209;
UPDATE map_pkmeters SET pkm_lat = 41.644044046006, pkm_lon = -0.88000945748736 WHERE pkm_uni_id = 60210;
UPDATE map_pkmeters SET pkm_lat = 41.644641348856, pkm_lon = -0.88082484902785 WHERE pkm_uni_id = 60211;
UPDATE map_pkmeters SET pkm_lat = 41.645038211442, pkm_lon = -0.88122181596200 WHERE pkm_uni_id = 60212;
UPDATE map_pkmeters SET pkm_lat = 41.645715678296, pkm_lon = -0.88150076569951 WHERE pkm_uni_id = 60213;
UPDATE map_pkmeters SET pkm_lat = 41.646260853551, pkm_lon = -0.88198892774020 WHERE pkm_uni_id = 60214;
UPDATE map_pkmeters SET pkm_lat = 41.646705809352, pkm_lon = -0.88272385301032 WHERE pkm_uni_id = 60215;
UPDATE map_pkmeters SET pkm_lat = 41.646096499733, pkm_lon = -0.88092140855231 WHERE pkm_uni_id = 60216;
UPDATE map_pkmeters SET pkm_lat = 41.645362915379, pkm_lon = -0.88059954347054 WHERE pkm_uni_id = 60217;
UPDATE map_pkmeters SET pkm_lat = 41.645030194041, pkm_lon = -0.87993435563490 WHERE pkm_uni_id = 60218;
UPDATE map_pkmeters SET pkm_lat = 41.644220431316, pkm_lon = -0.87958030404499 WHERE pkm_uni_id = 60219;
UPDATE map_pkmeters SET pkm_lat = 41.646256844927, pkm_lon = -0.88067464532297 WHERE pkm_uni_id = 60220;
UPDATE map_pkmeters SET pkm_lat = 41.645876024439, pkm_lon = -0.87969295682357 WHERE pkm_uni_id = 60221;
UPDATE map_pkmeters SET pkm_lat = 41.645258689603, pkm_lon = -0.87947301568435 WHERE pkm_uni_id = 60222;
UPDATE map_pkmeters SET pkm_lat = 41.644677427371, pkm_lon = -0.87856106461933 WHERE pkm_uni_id = 60223;
UPDATE map_pkmeters SET pkm_lat = 41.644623375057, pkm_lon = -0.88639033116749 WHERE pkm_uni_id = 70101;
UPDATE map_pkmeters SET pkm_lat = 41.644976142022, pkm_lon = -0.88657272138050 WHERE pkm_uni_id = 70102;
UPDATE map_pkmeters SET pkm_lat = 41.645092394351, pkm_lon = -0.88805866517471 WHERE pkm_uni_id = 70103;
UPDATE map_pkmeters SET pkm_lat = 41.644446990849, pkm_lon = -0.88790309705184 WHERE pkm_uni_id = 70104;
UPDATE map_pkmeters SET pkm_lat = 41.644463025797, pkm_lon = -0.88717890061784 WHERE pkm_uni_id = 70105;
UPDATE map_pkmeters SET pkm_lat = 41.645128472615, pkm_lon = -0.88705551900316 WHERE pkm_uni_id = 70106;
UPDATE map_pkmeters SET pkm_lat = 41.645922189351, pkm_lon = -0.88721645154405 WHERE pkm_uni_id = 70107;
UPDATE map_pkmeters SET pkm_lat = 41.642947705590, pkm_lon = -0.88796747006815 WHERE pkm_uni_id = 70108;
UPDATE map_pkmeters SET pkm_lat = 41.643236340757, pkm_lon = -0.88901889600194 WHERE pkm_uni_id = 70109;
UPDATE map_pkmeters SET pkm_lat = 41.644146334839, pkm_lon = -0.89043510236182 WHERE pkm_uni_id = 70110;
UPDATE map_pkmeters SET pkm_lat = 41.644561764005, pkm_lon = -0.89079735234664 WHERE pkm_uni_id = 70111;
UPDATE map_pkmeters SET pkm_lat = 41.644095971931, pkm_lon = -0.88719540128158 WHERE pkm_uni_id = 70201;
UPDATE map_pkmeters SET pkm_lat = 41.644059893087, pkm_lon = -0.88896565923134 WHERE pkm_uni_id = 70202;
UPDATE map_pkmeters SET pkm_lat = 41.644278370222, pkm_lon = -0.88997953423896 WHERE pkm_uni_id = 70203;
UPDATE map_pkmeters SET pkm_lat = 41.643450558449, pkm_lon = -0.88907294759193 WHERE pkm_uni_id = 70204;
UPDATE map_pkmeters SET pkm_lat = 41.643298223890, pkm_lon = -0.88730805406018 WHERE pkm_uni_id = 70205;
UPDATE map_pkmeters SET pkm_lat = 41.643422496847, pkm_lon = -0.88660531529830 WHERE pkm_uni_id = 70206;
UPDATE map_pkmeters SET pkm_lat = 41.643755226489, pkm_lon = -0.88827901372356 WHERE pkm_uni_id = 70207;
UPDATE map_pkmeters SET pkm_lat = 41.642813156182, pkm_lon = -0.88668578156875 WHERE pkm_uni_id = 70208;
UPDATE map_pkmeters SET pkm_lat = 41.643157915423, pkm_lon = -0.88778012284676 WHERE pkm_uni_id = 70209;
UPDATE map_pkmeters SET pkm_lat = 41.645951520579, pkm_lon = -0.89030239533827 WHERE pkm_uni_id = 80101;
UPDATE map_pkmeters SET pkm_lat = 41.646488676617, pkm_lon = -0.88946554612558 WHERE pkm_uni_id = 80102;
UPDATE map_pkmeters SET pkm_lat = 41.647274359473, pkm_lon = -0.88891837548655 WHERE pkm_uni_id = 80103;
UPDATE map_pkmeters SET pkm_lat = 41.647663188771, pkm_lon = -0.88749144029068 WHERE pkm_uni_id = 80104;
UPDATE map_pkmeters SET pkm_lat = 41.646127900667, pkm_lon = -0.88930461358470 WHERE pkm_uni_id = 80105;
UPDATE map_pkmeters SET pkm_lat = 41.646232125038, pkm_lon = -0.89119288873110 WHERE pkm_uni_id = 80106;
UPDATE map_pkmeters SET pkm_lat = 41.646697124023, pkm_lon = -0.89098367642795 WHERE pkm_uni_id = 80107;
UPDATE map_pkmeters SET pkm_lat = 41.646436564660, pkm_lon = -0.89011464070716 WHERE pkm_uni_id = 80108;
UPDATE map_pkmeters SET pkm_lat = 41.645127738999, pkm_lon = -0.89220676373876 WHERE pkm_uni_id = 80109;
UPDATE map_pkmeters SET pkm_lat = 41.646099840231, pkm_lon = -0.89198682259955 WHERE pkm_uni_id = 80110;
UPDATE map_pkmeters SET pkm_lat = 41.646957682331, pkm_lon = -0.89238378953366 WHERE pkm_uni_id = 80111;
UPDATE map_pkmeters SET pkm_lat = 41.647274359472, pkm_lon = -0.89149329614072 WHERE pkm_uni_id = 80112;
UPDATE map_pkmeters SET pkm_lat = 41.647675214381, pkm_lon = -0.89077446412475 WHERE pkm_uni_id = 80113;
UPDATE map_pkmeters SET pkm_lat = 41.645933481679, pkm_lon = -0.89330110501679 WHERE pkm_uni_id = 80114;
UPDATE map_pkmeters SET pkm_lat = 41.646709149811, pkm_lon = -0.89155230473899 WHERE pkm_uni_id = 80301;
UPDATE map_pkmeters SET pkm_lat = 41.645137524497, pkm_lon = -0.89186268271833 WHERE pkm_uni_id = 80201;
UPDATE map_pkmeters SET pkm_lat = 41.645768890791, pkm_lon = -0.89149522008336 WHERE pkm_uni_id = 80202;
UPDATE map_pkmeters SET pkm_lat = 41.647384358593, pkm_lon = -0.89290069760708 WHERE pkm_uni_id = 80203;
UPDATE map_pkmeters SET pkm_lat = 41.647396384256, pkm_lon = -0.89223550977142 WHERE pkm_uni_id = 80204;
UPDATE map_pkmeters SET pkm_lat = 41.647725084843, pkm_lon = -0.89131819428836 WHERE pkm_uni_id = 80205;
UPDATE map_pkmeters SET pkm_lat = 41.648025724155, pkm_lon = -0.89058326901831 WHERE pkm_uni_id = 80206;
UPDATE map_pkmeters SET pkm_lat = 41.647725084843, pkm_lon = -0.89245545091063 WHERE pkm_uni_id = 80207;
UPDATE map_pkmeters SET pkm_lat = 41.648158005008, pkm_lon = -0.88826584042953 WHERE pkm_uni_id = 80208;
UPDATE map_pkmeters SET pkm_lat = 41.648626998571, pkm_lon = -0.88754700841357 WHERE pkm_uni_id = 80209;
UPDATE map_pkmeters SET pkm_lat = 41.648831430594, pkm_lon = -0.88821219624924 WHERE pkm_uni_id = 80210;
UPDATE map_pkmeters SET pkm_lat = 41.645251772190, pkm_lon = -0.89173125447664 WHERE pkm_uni_id = 80211;
UPDATE map_pkmeters SET pkm_lat = 41.648940121947, pkm_lon = -0.89104257192057 WHERE pkm_uni_id = 90101;
UPDATE map_pkmeters SET pkm_lat = 41.649930203443, pkm_lon = -0.89019499387184 WHERE pkm_uni_id = 90102;
UPDATE map_pkmeters SET pkm_lat = 41.649898136264, pkm_lon = -0.88911138142986 WHERE pkm_uni_id = 90103;
UPDATE map_pkmeters SET pkm_lat = 41.648896028893, pkm_lon = -0.88913283910199 WHERE pkm_uni_id = 90104;
UPDATE map_pkmeters SET pkm_lat = 41.649280839968, pkm_lon = -0.89015207852760 WHERE pkm_uni_id = 90105;
UPDATE map_pkmeters SET pkm_lat = 41.648358892931, pkm_lon = -0.89348338212409 WHERE pkm_uni_id = 90106;
UPDATE map_pkmeters SET pkm_lat = 41.650186740301, pkm_lon = -0.89357457723058 WHERE pkm_uni_id = 90107;
UPDATE map_pkmeters SET pkm_lat = 41.651267794015, pkm_lon = -0.89139347733902 WHERE pkm_uni_id = 90201;
UPDATE map_pkmeters SET pkm_lat = 41.650762744586, pkm_lon = -0.89213376702708 WHERE pkm_uni_id = 90202;
UPDATE map_pkmeters SET pkm_lat = 41.650245666070, pkm_lon = -0.89149003686353 WHERE pkm_uni_id = 90203;
UPDATE map_pkmeters SET pkm_lat = 41.649668457241, pkm_lon = -0.89124327363417 WHERE pkm_uni_id = 90204;
UPDATE map_pkmeters SET pkm_lat = 41.650157481722, pkm_lon = -0.89239125909250 WHERE pkm_uni_id = 90205;
UPDATE map_pkmeters SET pkm_lat = 41.652177674659, pkm_lon = -0.89337294759193 WHERE pkm_uni_id = 90206;
UPDATE map_pkmeters SET pkm_lat = 41.644432894801, pkm_lon = -0.89256572732384 WHERE pkm_uni_id = 100101;
UPDATE map_pkmeters SET pkm_lat = 41.643434711221, pkm_lon = -0.89349913606098 WHERE pkm_uni_id = 100102;
UPDATE map_pkmeters SET pkm_lat = 41.643230262073, pkm_lon = -0.89399802693774 WHERE pkm_uni_id = 100103;
UPDATE map_pkmeters SET pkm_lat = 41.642360343745, pkm_lon = -0.89450764665059 WHERE pkm_uni_id = 100104;
UPDATE map_pkmeters SET pkm_lat = 41.642103775728, pkm_lon = -0.89473831662585 WHERE pkm_uni_id = 100105;
UPDATE map_pkmeters SET pkm_lat = 41.643550966329, pkm_lon = -0.89414286622454 WHERE pkm_uni_id = 100106;
UPDATE map_pkmeters SET pkm_lat = 41.644184352542, pkm_lon = -0.89489388474877 WHERE pkm_uni_id = 100107;
UPDATE map_pkmeters SET pkm_lat = 41.644472982170, pkm_lon = -0.89594531068257 WHERE pkm_uni_id = 100108;
UPDATE map_pkmeters SET pkm_lat = 41.644701479709, pkm_lon = -0.89643347272325 WHERE pkm_uni_id = 100109;
UPDATE map_pkmeters SET pkm_lat = 41.645078298436, pkm_lon = -0.89717376241119 WHERE pkm_uni_id = 100110;
UPDATE map_pkmeters SET pkm_lat = 41.645358906701, pkm_lon = -0.89734005937017 WHERE pkm_uni_id = 100111;
UPDATE map_pkmeters SET pkm_lat = 41.644877863216, pkm_lon = -0.89982914933590 WHERE pkm_uni_id = 100112;
UPDATE map_pkmeters SET pkm_lat = 41.644489017108, pkm_lon = -0.89905130872160 WHERE pkm_uni_id = 100113;
UPDATE map_pkmeters SET pkm_lat = 41.644456947222, pkm_lon = -0.89855778226288 WHERE pkm_uni_id = 100114;
UPDATE map_pkmeters SET pkm_lat = 41.643631142142, pkm_lon = -0.89709866055892 WHERE pkm_uni_id = 100115;
UPDATE map_pkmeters SET pkm_lat = 41.643502860790, pkm_lon = -0.89681434640336 WHERE pkm_uni_id = 100116;
UPDATE map_pkmeters SET pkm_lat = 41.642632946144, pkm_lon = -0.89564490327285 WHERE pkm_uni_id = 100117;
UPDATE map_pkmeters SET pkm_lat = 41.642556777943, pkm_lon = -0.89507091054369 WHERE pkm_uni_id = 100118;
UPDATE map_pkmeters SET pkm_lat = 41.643037838756, pkm_lon = -0.89467930802753 WHERE pkm_uni_id = 100119;
UPDATE map_pkmeters SET pkm_lat = 41.643907747938, pkm_lon = -0.89379417905261 WHERE pkm_uni_id = 100120;
UPDATE map_pkmeters SET pkm_lat = 41.644164308772, pkm_lon = -0.89357423791340 WHERE pkm_uni_id = 100121;
UPDATE map_pkmeters SET pkm_lat = 41.643382596784, pkm_lon = -0.89570391187156 WHERE pkm_uni_id = 100122;
UPDATE map_pkmeters SET pkm_lat = 41.645062263642, pkm_lon = -0.89744198331268 WHERE pkm_uni_id = 100123;
UPDATE map_pkmeters SET pkm_lat = 41.644332676259, pkm_lon = -0.89815008649263 WHERE pkm_uni_id = 100124;
UPDATE map_pkmeters SET pkm_lat = 41.645823911983, pkm_lon = -0.89622426042044 WHERE pkm_uni_id = 100125;
UPDATE map_pkmeters SET pkm_lat = 41.645226620096, pkm_lon = -0.89575755605186 WHERE pkm_uni_id = 100126;
UPDATE map_pkmeters SET pkm_lat = 41.644665401205, pkm_lon = -0.89455592641281 WHERE pkm_uni_id = 100127;
UPDATE map_pkmeters SET pkm_lat = 41.647074599210, pkm_lon = -0.89466857919138 WHERE pkm_uni_id = 100128;
UPDATE map_pkmeters SET pkm_lat = 41.642452547626, pkm_lon = -0.89534986028122 WHERE pkm_uni_id = 100129;
UPDATE map_pkmeters SET pkm_lat = 41.644180343788, pkm_lon = -0.89536058911734 WHERE pkm_uni_id = 100301;
UPDATE map_pkmeters SET pkm_lat = 41.643202200376, pkm_lon = -0.89670169362468 WHERE pkm_uni_id = 100302;
UPDATE map_pkmeters SET pkm_lat = 41.643891712846, pkm_lon = -0.89794623860750 WHERE pkm_uni_id = 100303;
UPDATE map_pkmeters SET pkm_lat = 41.642271756083, pkm_lon = -0.89637475842880 WHERE pkm_uni_id = 100201;
UPDATE map_pkmeters SET pkm_lat = 41.642961278516, pkm_lon = -0.89722233647748 WHERE pkm_uni_id = 100202;
UPDATE map_pkmeters SET pkm_lat = 41.643462380306, pkm_lon = -0.89817183846872 WHERE pkm_uni_id = 100203;
UPDATE map_pkmeters SET pkm_lat = 41.643751013167, pkm_lon = -0.89915352696818 WHERE pkm_uni_id = 100204;
UPDATE map_pkmeters SET pkm_lat = 41.644039644736, pkm_lon = -0.89969533318923 WHERE pkm_uni_id = 100205;
UPDATE map_pkmeters SET pkm_lat = 41.645174114619, pkm_lon = -0.89872973794377 WHERE pkm_uni_id = 100206;
UPDATE map_pkmeters SET pkm_lat = 41.644155898752, pkm_lon = -0.89767294759219 WHERE pkm_uni_id = 100207;
UPDATE map_pkmeters SET pkm_lat = 41.644789279020, pkm_lon = -0.89725452298588 WHERE pkm_uni_id = 100208;
UPDATE map_pkmeters SET pkm_lat = 41.645214201528, pkm_lon = -0.89639085168300 WHERE pkm_uni_id = 100209;
UPDATE map_pkmeters SET pkm_lat = 41.645855588694, pkm_lon = -0.89595096940458 WHERE pkm_uni_id = 100210;
UPDATE map_pkmeters SET pkm_lat = 41.646525029738, pkm_lon = -0.89500146741331 WHERE pkm_uni_id = 100211;
UPDATE map_pkmeters SET pkm_lat = 41.646288521421, pkm_lon = -0.89558618897854 WHERE pkm_uni_id = 100212;
UPDATE map_pkmeters SET pkm_lat = 41.646504986694, pkm_lon = -0.89465278024139 WHERE pkm_uni_id = 100213;
UPDATE map_pkmeters SET pkm_lat = 41.645406618349, pkm_lon = -0.89423972005314 WHERE pkm_uni_id = 100214;
UPDATE map_pkmeters SET pkm_lat = 41.644749191841, pkm_lon = -0.89481907720021 WHERE pkm_uni_id = 100215;
UPDATE map_pkmeters SET pkm_lat = 41.643290001729, pkm_lon = -0.89625137681423 WHERE pkm_uni_id = 100216;
UPDATE map_pkmeters SET pkm_lat = 41.649473903214, pkm_lon = -0.90365766915709 WHERE pkm_uni_id = 110101;
UPDATE map_pkmeters SET pkm_lat = 41.650439926324, pkm_lon = -0.90410291585362 WHERE pkm_uni_id = 110102;
UPDATE map_pkmeters SET pkm_lat = 41.651205519413, pkm_lon = -0.90445696744352 WHERE pkm_uni_id = 110103;
UPDATE map_pkmeters SET pkm_lat = 41.652279739401, pkm_lon = -0.90493440064815 WHERE pkm_uni_id = 110104;
UPDATE map_pkmeters SET pkm_lat = 41.652977171142, pkm_lon = -0.90455889138608 WHERE pkm_uni_id = 110105;
UPDATE map_pkmeters SET pkm_lat = 41.652660522033, pkm_lon = -0.90417265328799 WHERE pkm_uni_id = 110106;
UPDATE map_pkmeters SET pkm_lat = 41.651606349857, pkm_lon = -0.90366839799303 WHERE pkm_uni_id = 110107;
UPDATE map_pkmeters SET pkm_lat = 41.651217544358, pkm_lon = -0.90324997338672 WHERE pkm_uni_id = 110108;
UPDATE map_pkmeters SET pkm_lat = 41.650203432374, pkm_lon = -0.90160846146973 WHERE pkm_uni_id = 110109;
UPDATE map_pkmeters SET pkm_lat = 41.649361667225, pkm_lon = -0.90123295220771 WHERE pkm_uni_id = 110110;
UPDATE map_pkmeters SET pkm_lat = 41.648972848178, pkm_lon = -0.90204834374822 WHERE pkm_uni_id = 110111;
UPDATE map_pkmeters SET pkm_lat = 41.650151323423, pkm_lon = -0.90114175710109 WHERE pkm_uni_id = 110112;
UPDATE map_pkmeters SET pkm_lat = 41.651049194855, pkm_lon = -0.90158700379764 WHERE pkm_uni_id = 110113;
UPDATE map_pkmeters SET pkm_lat = 41.651726598502, pkm_lon = -0.90189814004331 WHERE pkm_uni_id = 110114;
UPDATE map_pkmeters SET pkm_lat = 41.652604406834, pkm_lon = -0.90227901372333 WHERE pkm_uni_id = 110115;
UPDATE map_pkmeters SET pkm_lat = 41.653406047827, pkm_lon = -0.90191423329742 WHERE pkm_uni_id = 110116;
UPDATE map_pkmeters SET pkm_lat = 41.653373982378, pkm_lon = -0.90132951173220 WHERE pkm_uni_id = 110117;
UPDATE map_pkmeters SET pkm_lat = 41.653766783018, pkm_lon = -0.90111493501109 WHERE pkm_uni_id = 110118;
UPDATE map_pkmeters SET pkm_lat = 41.652395978538, pkm_lon = -0.90062140855237 WHERE pkm_uni_id = 110119;
UPDATE map_pkmeters SET pkm_lat = 41.651834822091, pkm_lon = -0.90061067971625 WHERE pkm_uni_id = 110120;
UPDATE map_pkmeters SET pkm_lat = 41.652742690614, pkm_lon = -0.90166210565002 WHERE pkm_uni_id = 110121;
UPDATE map_pkmeters SET pkm_lat = 41.652440069194, pkm_lon = -0.90289055737876 WHERE pkm_uni_id = 110122;
UPDATE map_pkmeters SET pkm_lat = 41.652055276998, pkm_lon = -0.90363084706689 WHERE pkm_uni_id = 110123;
UPDATE map_pkmeters SET pkm_lat = 41.649125169321, pkm_lon = -0.89989721211832 WHERE pkm_uni_id = 110124;
UPDATE map_pkmeters SET pkm_lat = 41.648660187860, pkm_lon = -0.90136706265847 WHERE pkm_uni_id = 110125;
UPDATE map_pkmeters SET pkm_lat = 41.650792661436, pkm_lon = -0.90186595353514 WHERE pkm_uni_id = 110301;
UPDATE map_pkmeters SET pkm_lat = 41.651882921414, pkm_lon = -0.90428530606661 WHERE pkm_uni_id = 110302;
UPDATE map_pkmeters SET pkm_lat = 41.649193229634, pkm_lon = -0.90303539666569 WHERE pkm_uni_id = 110201;
UPDATE map_pkmeters SET pkm_lat = 41.650231407791, pkm_lon = -0.90329288873107 WHERE pkm_uni_id = 110202;
UPDATE map_pkmeters SET pkm_lat = 41.650924853378, pkm_lon = -0.90358793172274 WHERE pkm_uni_id = 110203;
UPDATE map_pkmeters SET pkm_lat = 41.650079089263, pkm_lon = -0.90273498925606 WHERE pkm_uni_id = 110204;
UPDATE map_pkmeters SET pkm_lat = 41.649509895788, pkm_lon = -0.90271353158395 WHERE pkm_uni_id = 110205;
UPDATE map_pkmeters SET pkm_lat = 41.650239424543, pkm_lon = -0.90218245419895 WHERE pkm_uni_id = 110206;
UPDATE map_pkmeters SET pkm_lat = 41.652872874451, pkm_lon = -0.90319632920654 WHERE pkm_uni_id = 110207;
UPDATE map_pkmeters SET pkm_lat = 41.651429901535, pkm_lon = -0.90107738408482 WHERE pkm_uni_id = 110208;
UPDATE map_pkmeters SET pkm_lat = 41.652347796382, pkm_lon = -0.90124904546179 WHERE pkm_uni_id = 110209;
UPDATE map_pkmeters SET pkm_lat = 41.651902879552, pkm_lon = -0.90298711690334 WHERE pkm_uni_id = 110210;
UPDATE map_pkmeters SET pkm_lat = 41.651257544275, pkm_lon = -0.90243458184630 WHERE pkm_uni_id = 110211;
UPDATE map_pkmeters SET pkm_lat = 41.647320049991, pkm_lon = -0.89713476304462 WHERE pkm_uni_id = 120101;
UPDATE map_pkmeters SET pkm_lat = 41.647925339518, pkm_lon = -0.89748345021645 WHERE pkm_uni_id = 120102;
UPDATE map_pkmeters SET pkm_lat = 41.648562691220, pkm_lon = -0.89770339135575 WHERE pkm_uni_id = 120103;
UPDATE map_pkmeters SET pkm_lat = 41.648274079913, pkm_lon = -0.89660905007772 WHERE pkm_uni_id = 120104;
UPDATE map_pkmeters SET pkm_lat = 41.648474504569, pkm_lon = -0.89701138142994 WHERE pkm_uni_id = 120105;
UPDATE map_pkmeters SET pkm_lat = 41.648658894702, pkm_lon = -0.89840076569956 WHERE pkm_uni_id = 120106;
UPDATE map_pkmeters SET pkm_lat = 41.648839275844, pkm_lon = -0.89915714864177 WHERE pkm_uni_id = 120107;
UPDATE map_pkmeters SET pkm_lat = 41.648330198880, pkm_lon = -0.89887283448620 WHERE pkm_uni_id = 120108;
UPDATE map_pkmeters SET pkm_lat = 41.647740947285, pkm_lon = -0.89828811292089 WHERE pkm_uni_id = 120109;
UPDATE map_pkmeters SET pkm_lat = 41.647396212563, pkm_lon = -0.89834712151920 WHERE pkm_uni_id = 120110;
UPDATE map_pkmeters SET pkm_lat = 41.646826995382, pkm_lon = -0.89775167111802 WHERE pkm_uni_id = 120111;
UPDATE map_pkmeters SET pkm_lat = 41.646502298810, pkm_lon = -0.89757464532310 WHERE pkm_uni_id = 120112;
UPDATE map_pkmeters SET pkm_lat = 41.648326190384, pkm_lon = -0.89941464070717 WHERE pkm_uni_id = 120113;
UPDATE map_pkmeters SET pkm_lat = 41.647344101343, pkm_lon = -0.89909814004336 WHERE pkm_uni_id = 120114;
UPDATE map_pkmeters SET pkm_lat = 41.646426135207, pkm_lon = -0.89972041253488 WHERE pkm_uni_id = 120115;
UPDATE map_pkmeters SET pkm_lat = 41.645744667166, pkm_lon = -0.90009055737890 WHERE pkm_uni_id = 120116;
UPDATE map_pkmeters SET pkm_lat = 41.646109453897, pkm_lon = -0.89913032655160 WHERE pkm_uni_id = 120117;
UPDATE map_pkmeters SET pkm_lat = 41.646341954236, pkm_lon = -0.89804671410971 WHERE pkm_uni_id = 120118;
UPDATE map_pkmeters SET pkm_lat = 41.647332075653, pkm_lon = -0.89454374913640 WHERE pkm_uni_id = 120119;
UPDATE map_pkmeters SET pkm_lat = 41.647291990061, pkm_lon = -0.89516065720980 WHERE pkm_uni_id = 120120;
UPDATE map_pkmeters SET pkm_lat = 41.648418385729, pkm_lon = -0.90111516122259 WHERE pkm_uni_id = 120121;
UPDATE map_pkmeters SET pkm_lat = 41.648871343549, pkm_lon = -0.89971504811687 WHERE pkm_uni_id = 120122;
UPDATE map_pkmeters SET pkm_lat = 41.646923201452, pkm_lon = -0.89717767838892 WHERE pkm_uni_id = 120301;
UPDATE map_pkmeters SET pkm_lat = 41.648390326289, pkm_lon = -0.89626572732383 WHERE pkm_uni_id = 120302;
UPDATE map_pkmeters SET pkm_lat = 41.646983964583, pkm_lon = -0.89650312417433 WHERE pkm_uni_id = 120201;
UPDATE map_pkmeters SET pkm_lat = 41.647701496341, pkm_lon = -0.89476505273275 WHERE pkm_uni_id = 120202;
UPDATE map_pkmeters SET pkm_lat = 41.648030195371, pkm_lon = -0.89502790921620 WHERE pkm_uni_id = 120203;
UPDATE map_pkmeters SET pkm_lat = 41.647677445127, pkm_lon = -0.89704493039532 WHERE pkm_uni_id = 120204;
UPDATE map_pkmeters SET pkm_lat = 41.647749598743, pkm_lon = -0.89600959771561 WHERE pkm_uni_id = 120205;

/* #################################################################################################################################################################### */
/* ### MAP_BASES ###################################################################################################################################################### */
/* #################################################################################################################################################################### */

/* ATENCION: Las siguientes bases son de prueba. */

DELETE FROM map_bases;

INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60001, 'Test1A' , 41.654186190736, -0.87768943753564);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60002, 'Test1B' , 41.652117529049, -0.87615028409326);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60005, 'Test3A' , 41.652813576260, -0.88869439454392);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60006, 'Test3B' , 41.654372239680, -0.89064208569840);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60007, 'Test4A' , 41.651247320372, -0.88204862733216);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60008, 'Test4B' , 41.647189190831, -0.88565340314233);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60009, 'Test5A' , 41.648746373446, -0.87908325829824);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60010, 'Test5B' , 41.649505678419, -0.87630419545504);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60011, 'Test6A1', 41.646152184527, -0.88507615031635);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60011, 'Test6A2', 41.642449883136, -0.88167839086235);
INSERT INTO map_bases (bas_grp_id, bas_name, bas_lat, bas_lon) VALUES (60012, 'Test6B' , 41.644152282501, -0.88346950711641);

/* #################################################################################################################################################################### */
/* ### MAP_GUARDS y MAP_ALARMS ######################################################################################################################################## */
/* #################################################################################################################################################################### */

/* ATENCION: Los siguientes vigilantes y sus alarmas son de prueba. */

DELETE FROM map_alarms;

DELETE FROM map_guards;

INSERT INTO map_guards(gua_id, gua_grp_id, gua_name, gua_surname1, gua_surname2, gua_photo, gua_uni_id, gua_ipdir, gua_ipdate, gua_poslat, gua_poslon, gua_posdate)
VALUES (99901, 60001, 'Luis', 'Sánchez', 'Navarro', NULL, 2001, '11.22.33.100', SYSDATE - (1/24/60) * 10, 41.65208, -0.87302, SYSDATE - (1/24/60) * 10);

INSERT INTO map_guards(gua_id, gua_grp_id, gua_name, gua_surname1, gua_surname2, gua_photo, gua_uni_id, gua_ipdir, gua_ipdate, gua_poslat, gua_poslon, gua_posdate)
VALUES (99902, 60001, 'Juan José', 'López', NULL, NULL, 2002, '11.22.33.101', SYSDATE - (1/24/60) * 15, 41.65268, -0.87942, SYSDATE - (1/24/60) * 15);

INSERT INTO map_guards(gua_id, gua_grp_id, gua_name, gua_surname1, gua_surname2, gua_photo, gua_uni_id, gua_ipdir, gua_ipdate, gua_poslat, gua_poslon, gua_posdate)
VALUES (99903, 60002, 'Felipe', 'Nuñez', 'Cano', NULL, 2003, '11.22.33.102', SYSDATE - (1/24/60) * 20, 41.65238, -0.87823, SYSDATE - (1/24/60) * 20);

INSERT INTO map_guards(gua_id, gua_grp_id, gua_name, gua_surname1, gua_surname2, gua_photo, gua_uni_id, gua_ipdir, gua_ipdate, gua_poslat, gua_poslon, gua_posdate)
VALUES (99904, 60002, 'Carles', 'Fabra', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

INSERT INTO map_alarms (alx_timestamp, alx_als_id, alx_ald_id, alx_all_id, alx_gua_id, alx_uni_id)
    SELECT (SYSDATE - (1/24/60) * 10), 0, 1, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 20), 0, 2, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 30), 0, 3, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 15), 0, 1, 2, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 25), 0, 2, 2, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 35), 0, 3, 2, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 70), 1, 1, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 80), 1, 2, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 90), 1, 3, 1, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 75), 1, 1, 2, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 85), 1, 2, 2, gua_id, gua_uni_id
    FROM map_guards
    UNION
    SELECT (SYSDATE - (1/24/60) * 95), 1, 3, 2, gua_id, gua_uni_id
    FROM map_guards;

/* #################################################################################################################################################################### */
/* ### FIN ############################################################################################################################################################ */
/* #################################################################################################################################################################### */

COMMIT WORK;

END;
/
/* #################################################################################################################################################################### */
/* #################################################################################################################################################################### */
/* #################################################################################################################################################################### */
