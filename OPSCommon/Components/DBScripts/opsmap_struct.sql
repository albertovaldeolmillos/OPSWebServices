/* #################################################################################################################################################################### */
/* ### MAP_ZONES ###################################################################################################################################################### */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_ZONES
(
  ZON_ID     NUMBER not null,
  ZON_GRP_ID NUMBER not null,
  ZON_LAT    NUMBER not null,
  ZON_LON    NUMBER not null,
  ZON_ZOOM   NUMBER not null
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_ZONES.ZON_ID
  is 'Identificador de zona';
comment on column MAP_ZONES.ZON_GRP_ID
  is 'Identificador de agrupacion';
comment on column MAP_ZONES.ZON_LAT
  is 'Latitud';
comment on column MAP_ZONES.ZON_LON
  is 'Longitud';
comment on column MAP_ZONES.ZON_ZOOM
  is 'Zoom';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_ZONES
  add constraint PK_MAP_ZONES primary key (ZON_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table MAP_ZONES
  add constraint FK_ZON_GRP_ID foreign key (ZON_GRP_ID)
  references GROUPS (GRP_ID);

-- Create/Recreate indexes
create unique index IDX_ZON_GRP_ID on MAP_ZONES (ZON_GRP_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create sequence
create sequence SEQ_MAP_ZONES
minvalue 1
maxvalue 9999999999999999999999999999
start with 1
increment by 1
cache 20;

-- Create trigger before insert
create or replace trigger TR_BI_MAP_ZONES
  before insert on map_zones
  for each row
declare
begin
  select seq_map_zones.nextval
  into :new.zon_id
  from dual;
end TR_BI_MAP_ZONES;
/

/* #################################################################################################################################################################### */
/* ### MAP_BASES ###################################################################################################################################################### */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_BASES
(
  BAS_ID      NUMBER not null,
  BAS_GRP_ID  NUMBER not null,
  BAS_NAME    VARCHAR2(50) not null,
  BAS_LAT     NUMBER not null,
  BAS_LON     NUMBER not null,
  BAS_VISIBLE NUMBER default 1 not null,
  BAS_DELETED NUMBER default 0 not null
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_BASES.BAS_ID
  is 'Identificador de la base';
comment on column MAP_BASES.BAS_GRP_ID
  is 'Identificador de la agrupacion';
comment on column MAP_BASES.BAS_NAME
  is 'Nombre de la base';
comment on column MAP_BASES.BAS_LAT
  is 'Latitud de la base';
comment on column MAP_BASES.BAS_LON
  is 'Longitud de la base';
comment on column MAP_BASES.BAS_VISIBLE
  is 'Visibilidad de la base en el mapa (0 = Invisible)';
comment on column MAP_BASES.BAS_DELETED
  is 'Marca de eliminacion logica (0 = Activo)';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_BASES
  add constraint PK_BAS_ID primary key (BAS_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table MAP_BASES
  add constraint FK_BAS_GRP_ID foreign key (BAS_GRP_ID)
  references GROUPS (GRP_ID);

-- Create/Recreate indexes
create index IDX_BAS_GRP_ID on MAP_BASES (BAS_GRP_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create sequence
create sequence SEQ_MAP_BASES
minvalue 1
maxvalue 9999999999999999999999999999
start with 1
increment by 1
cache 20;

-- Create trigger before insert
create or replace trigger TR_BI_MAP_BASES
  before insert on map_bases
  for each row
declare
begin
  select seq_map_bases.nextval
  into :new.bas_id
  from dual;
end TR_BI_MAP_BASES;
/

/* #################################################################################################################################################################### */
/* ### MAP_PKMETERS ################################################################################################################################################### */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_PKMETERS
(
  PKM_ID     NUMBER not null,
  PKM_UNI_ID NUMBER not null,
  PKM_GRP_ID NUMBER not null,
  PKM_LAT    NUMBER,
  PKM_LON    NUMBER
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_PKMETERS.PKM_ID
  is 'Identificador de parquimetro';
comment on column MAP_PKMETERS.PKM_UNI_ID
  is 'Identificador de la unidad';
comment on column MAP_PKMETERS.PKM_GRP_ID
  is 'Identificador de la agrupacion';
comment on column MAP_PKMETERS.PKM_LAT
  is 'Latitud del parquimetro';
comment on column MAP_PKMETERS.PKM_LON
  is 'Longitud del parquimetro';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_PKMETERS
  add constraint PK_PKM_ID primary key (PKM_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table MAP_PKMETERS
  add constraint FK_PKM_GRP_ID foreign key (PKM_GRP_ID)
  references GROUPS (GRP_ID);
alter table MAP_PKMETERS
  add constraint FK_PKM_UNI_ID foreign key (PKM_UNI_ID)
  references UNITS (UNI_ID);

-- Create/Recreate indexes
create index IDX_PKM_GRP_ID on MAP_PKMETERS (PKM_GRP_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create unique index IDX_PKM_UNI_ID on MAP_PKMETERS (PKM_UNI_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create sequence
create sequence SEQ_MAP_PKMETERS
minvalue 1
maxvalue 9999999999999999999999999999
start with 1
increment by 1
cache 20;

-- Create trigger before insert
create or replace trigger TR_BI_MAP_PKMETERS
  before insert on MAP_PKMETERS
  for each row
declare
begin
  select seq_MAP_PKMETERS.nextval
  into :new.pkm_id
  from dual;
end TR_BI_MAP_PKMETERS;
/

/* #################################################################################################################################################################### */
/* ### MAP_GUARDS ##################################################################################################################################################### */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_GUARDS
(
  GUA_ID       NUMBER not null,
  GUA_GRP_ID   NUMBER not null,
  GUA_NAME     VARCHAR2(60) not null,
  GUA_SURNAME1 VARCHAR2(20) not null,
  GUA_SURNAME2 VARCHAR2(20),
  GUA_PHOTO    VARCHAR2(100),
  GUA_UNI_ID   NUMBER,
  GUA_IPDIR    VARCHAR2(20),
  GUA_IPDATE   DATE,
  GUA_POSLAT   NUMBER,
  GUA_POSLON   NUMBER,
  GUA_POSDATE  DATE,
  GUA_DELETED  NUMBER default 0 not null,
  GUA_STATUS   NUMBER default 0 not null
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_GUARDS.GUA_ID
  is 'Identificador del vigilante';
comment on column MAP_GUARDS.GUA_GRP_ID
  is 'Identificador de la agrupacion';
comment on column MAP_GUARDS.GUA_NAME
  is 'Nombre del vigilante';
comment on column MAP_GUARDS.GUA_SURNAME1
  is 'Primer apellido del vigilante';
comment on column MAP_GUARDS.GUA_SURNAME2
  is 'Segundo apellido del vigilante';
comment on column MAP_GUARDS.GUA_PHOTO
  is 'Foto del vigilante (Nombre del archivo -sin ruta-)';
comment on column MAP_GUARDS.GUA_UNI_ID
  is 'Identificador de ultima unidad PDA utilizada por el vigilante';
comment on column MAP_GUARDS.GUA_IPDIR
  is 'Ultima dirección IP de la PDA';
comment on column MAP_GUARDS.GUA_IPDATE
  is 'Ultima fecha de actualizacion de la IP';
comment on column MAP_GUARDS.GUA_POSLAT
  is 'Ultima latitud reportada por la PDA';
comment on column MAP_GUARDS.GUA_POSLON
  is 'Ultima longitud reportada por la PDA';
comment on column MAP_GUARDS.GUA_POSDATE
  is 'Ultima fecha de actualización de latitud y longitud';
comment on column MAP_GUARDS.GUA_DELETED
  is 'Marca de eliminacion logica (0 = Activo)';
comment on column MAP_GUARDS.GUA_STATUS
  is 'Estado (0 = Posición Inválida, 1 = Posición Válida Inactiva, 2 = Posición Válida Activa)';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_GUARDS
  add constraint PK_MAP_GUARDS primary key (GUA_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table MAP_GUARDS
  add constraint FK_GUA_GRP_ID foreign key (GUA_GRP_ID)
  references GROUPS (GRP_ID);
alter table MAP_GUARDS
  add constraint FK_GUA_UNI_ID foreign key (GUA_UNI_ID)
  references UNITS (UNI_ID);

-- Create/Recreate indexes
create index IDX_GUA_GRP_ID on MAP_GUARDS (GUA_GRP_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_GUA_UNI_ID on MAP_GUARDS (GUA_UNI_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

/

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_DEF ################################################################################################################################################# */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_ALARMS_DEF
(
  ALD_ID          NUMBER not null,
  ALD_NAME        VARCHAR2(20) not null,
  ALD_NAME_LIT_ID NUMBER
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_ALARMS_DEF.ALD_ID
  is 'Identificador de tipo de alarma';
comment on column MAP_ALARMS_DEF.ALD_NAME
  is 'Nombre del tipo de alarma';
comment on column MAP_ALARMS_DEF.ALD_NAME_LIT_ID
  is 'Identificador de literal para localizacion del nombre';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_ALARMS_DEF
  add constraint PK_MAP_ALARMS_DEF unique (ALD_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create/Recreate indexes
create unique index IDX_ALD_NAME on MAP_ALARMS_DEF (ALD_NAME)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_STATUS ############################################################################################################################################## */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_ALARMS_STATUS
(
  ALS_ID          NUMBER not null,
  ALS_NAME        VARCHAR2(20) not null,
  ALS_NAME_LIT_ID NUMBER
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_ALARMS_STATUS.ALS_ID
  is 'Identificador de estado de alarma';
comment on column MAP_ALARMS_STATUS.ALS_NAME
  is 'Nombre del estado de alarma';
comment on column MAP_ALARMS_STATUS.ALS_NAME_LIT_ID
  is 'Identificador de literal para localizacion del nombre';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_ALARMS_STATUS
  add constraint PK_MAP_ALARMS_STATUS unique (ALS_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create/Recreate indexes
create unique index IDX_ALS_NAME on MAP_ALARMS_STATUS (ALS_NAME)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS_LEVEL ############################################################################################################################################## */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_ALARMS_LEVEL
(
  ALL_ID          NUMBER not null,
  ALL_NAME        VARCHAR2(20) not null,
  ALL_NAME_LIT_ID NUMBER
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_ALARMS_LEVEL.ALL_ID
  is 'Identificador de nivel de alarma';
comment on column MAP_ALARMS_LEVEL.ALL_NAME
  is 'Nombre del nivel de alarma';
comment on column MAP_ALARMS_LEVEL.ALL_NAME_LIT_ID
  is 'Identificador de literal para localizacion del nombre';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_ALARMS_LEVEL
  add constraint PK_MAP_ALARMS_LEVEL unique (ALL_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create/Recreate indexes
create unique index IDX_ALL_NAME on MAP_ALARMS_LEVEL (ALL_NAME)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

/* #################################################################################################################################################################### */
/* ### MAP_ALARMS ##################################################################################################################################################### */
/* #################################################################################################################################################################### */

-- Create table
create table MAP_ALARMS
(
  ALX_ID        NUMBER not null,
  ALX_TIMESTAMP DATE default SYSDATE not null,
  ALX_ALS_ID    NUMBER not null,
  ALX_ALD_ID    NUMBER not null,
  ALX_ALL_ID    NUMBER not null,
  ALX_GUA_ID    NUMBER not null,
  ALX_UNI_ID    NUMBER
)
tablespace USERS
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Add comments to the columns
comment on column MAP_ALARMS.ALX_ID
  is 'Identificador de alarma';
comment on column MAP_ALARMS.ALX_TIMESTAMP
  is 'Fecha de generacion de la alarma';
comment on column MAP_ALARMS.ALX_ALS_ID
  is 'Identificador de estado de alarma';
comment on column MAP_ALARMS.ALX_ALD_ID
  is 'Identificador de tipo de alarma';
comment on column MAP_ALARMS.ALX_ALL_ID
  is 'Identificador de nivel de alarma';
comment on column MAP_ALARMS.ALX_GUA_ID
  is 'Identificador de vigilante logueado a la PDA';
comment on column MAP_ALARMS.ALX_UNI_ID
  is 'Identificador de unidad PDA generadora de la alarma';

-- Create/Recreate primary, unique and foreign key constraints
alter table MAP_ALARMS
  add constraint PK_MAP_ALARMS primary key (ALX_ID)
  using index
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
alter table MAP_ALARMS
  add constraint FK_ALX_ALD_ID foreign key (ALX_ALD_ID)
  references MAP_ALARMS_DEF (ALD_ID);
alter table MAP_ALARMS
  add constraint FK_ALX_ALL_ID foreign key (ALX_ALL_ID)
  references MAP_ALARMS_LEVEL (ALL_ID);
alter table MAP_ALARMS
  add constraint FK_ALX_ALS_ID foreign key (ALX_ALS_ID)
  references MAP_ALARMS_STATUS (ALS_ID);
alter table MAP_ALARMS
  add constraint FK_ALX_GUA_ID foreign key (ALX_GUA_ID)
  references MAP_GUARDS (GUA_ID);
alter table MAP_ALARMS
  add constraint FK_ALX_UNI_ID foreign key (ALX_UNI_ID)
  references UNITS (UNI_ID);

-- Create/Recreate indexes
create index IDX_ALX_ALD_ID on MAP_ALARMS (ALX_ALD_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_ALX_ALL_ID on MAP_ALARMS (ALX_ALL_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_ALX_ALS_ID on MAP_ALARMS (ALX_ALS_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_ALX_GUA_ID on MAP_ALARMS (ALX_GUA_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_ALX_TIMESTAMP on MAP_ALARMS (ALX_TIMESTAMP)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );
create index IDX_ALX_UNI_ID on MAP_ALARMS (ALX_UNI_ID)
  tablespace USERS
  pctfree 10
  initrans 2
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  );

-- Create sequence
create sequence SEQ_MAP_ALARMS
minvalue 1
maxvalue 9999999999999999999999999999
start with 1
increment by 1
cache 20;

-- Create trigger before insert
create or replace trigger TR_BI_MAP_ALARMS
  before insert on MAP_ALARMS
  for each row
declare
begin
  select SEQ_MAP_ALARMS.nextval
  into :new.alx_id
  from dual;
end TR_BI_MAP_ALARMS;

-- Create trigger after update
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

/* #################################################################################################################################################################### */
/* #################################################################################################################################################################### */
/* #################################################################################################################################################################### */
