-------------------------------------------------
-- Export file for user LM04                   --
-- Created by zhang_work on 2020/4/1, 11:14:10 --
-------------------------------------------------

set define off
spool new_object.log

prompt
prompt Creating table DNCUSER
prompt ======================
prompt
create table LM04.DNCUSER
(
  username  VARCHAR2(32) not null,
  passwd    VARCHAR2(32),
  authority NUMBER(10)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.DNCUSER
  add constraint DNCUSER_ID primary key (USERNAME)
  using index 
  tablespace SYSTEM
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

prompt
prompt Creating table EQUIPMENT
prompt ========================
prompt
create table LM04.EQUIPMENT
(
  mach_num             VARCHAR2(64),
  equipment_name       VARCHAR2(64),
  equipment_type       VARCHAR2(64),
  manufacturer         VARCHAR2(128),
  check_date           NUMBER,
  open_date            NUMBER,
  use_unit             VARCHAR2(128),
  spindle_speed        VARCHAR2(64),
  picture_path         VARCHAR2(128),
  next_check           NUMBER,
  travel               VARCHAR2(64),
  nc_system            VARCHAR2(64),
  positioning_accuracy VARCHAR2(64)
)
tablespace CNC
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

prompt
prompt Creating table EQUIPMENT_RECORD
prompt ===============================
prompt
create table LM04.EQUIPMENT_RECORD
(
  mach_num   VARCHAR2(64),
  check_info VARCHAR2(256),
  check_time NUMBER,
  remark     VARCHAR2(256)
)
tablespace CNC
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

prompt
prompt Creating table ROOM_MACHINE_HISTORY
prompt ===================================
prompt
create table LM04.ROOM_MACHINE_HISTORY
(
  mach_num     VARCHAR2(64) not null,
  createuserid VARCHAR2(32),
  createdate   DATE,
  removeuserid VARCHAR2(32),
  removedate   DATE,
  ip           VARCHAR2(16),
  iport        NUMBER,
  itype        NUMBER,
  machine_type VARCHAR2(32),
  system_name  VARCHAR2(32),
  room_id      VARCHAR2(32),
  machine_name VARCHAR2(32),
  family       VARCHAR2(128),
  channo       NUMBER,
  id           VARCHAR2(32)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.ROOM_MACHINE_HISTORY
  add constraint MACH_NUM6 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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

prompt
prompt Creating table ERROR
prompt ====================
prompt
create table LM04.ERROR
(
  log_id      NUMBER not null,
  mach_num    VARCHAR2(64),
  error_id    VARCHAR2(32),
  error_type  NUMBER,
  start_time  DATE,
  end_time    DATE,
  last_time   NUMBER,
  date_zone   DATE,
  error_info  VARCHAR2(128),
  description VARCHAR2(128)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.ERROR
  add constraint LOG_ID primary key (LOG_ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.ERROR
  add constraint MACHNUM_FERROR foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table ERROR_1
prompt ======================
prompt
create table LM04.ERROR_1
(
  errorid     NUMBER,
  type        NUMBER,
  starttime   DATE,
  endtime     DATE,
  errorinfo   VARCHAR2(256),
  description VARCHAR2(128)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );

prompt
prompt Creating table ERROR_SET_1
prompt ==========================
prompt
create table LM04.ERROR_SET_1
(
  error_id VARCHAR2(32)
)
tablespace CNC
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

prompt
prompt Creating table FIX_RECORD
prompt =========================
prompt
create table LM04.FIX_RECORD
(
  fix_id     NUMBER not null,
  mach_num   VARCHAR2(64),
  start_time NUMBER,
  end_time   NUMBER,
  company    VARCHAR2(128),
  record     VARCHAR2(128),
  type       NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.FIX_RECORD
  add constraint FIX_ID primary key (FIX_ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.FIX_RECORD
  add constraint MACH_NUM8 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table FIX_RECORD_SYS
prompt =============================
prompt
create table LM04.FIX_RECORD_SYS
(
  fix_id     NUMBER not null,
  mach_num   VARCHAR2(64) not null,
  start_time NUMBER,
  end_time   NUMBER,
  company    VARCHAR2(128),
  record     VARCHAR2(128),
  type       NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.FIX_RECORD_SYS
  add constraint FIX_ID1 primary key (FIX_ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.FIX_RECORD_SYS
  add constraint MACH_NUM9 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table GROUP_INFO
prompt =========================
prompt
create table LM04.GROUP_INFO
(
  groupname VARCHAR2(64),
  workshop  VARCHAR2(64) not null
)
tablespace CNC
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

prompt
prompt Creating table LOOK_HISTORY
prompt ===========================
prompt
create table LM04.LOOK_HISTORY
(
  id        NUMBER not null,
  consumer  VARCHAR2(32),
  look_time NUMBER,
  rank      NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.LOOK_HISTORY
  add constraint LOOK_HISTORY_ID primary key (ID)
  using index 
  tablespace SYSTEM
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

prompt
prompt Creating table MACHINE_ERROR_GOLDING
prompt ====================================
prompt
create table LM04.MACHINE_ERROR_GOLDING
(
  error_id NUMBER
)
tablespace CNC
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

prompt
prompt Creating table MACHINE_INFO
prompt ===========================
prompt
create table LM04.MACHINE_INFO
(
  mach_num                VARCHAR2(64) not null,
  cnc_system              VARCHAR2(32),
  cnc_system_num          VARCHAR2(32),
  mach_name               VARCHAR2(64),
  mach_type               VARCHAR2(64),
  manufactured_date       DATE,
  picture_big_path        VARCHAR2(128),
  processing_cost         NUMBER(10),
  manufacturer            VARCHAR2(128),
  cnc_manu_date           DATE,
  range_body_d            VARCHAR2(64),
  range_body_d_u          VARCHAR2(32),
  range_saddle_d          VARCHAR2(64),
  range_saddle_d_u        VARCHAR2(32),
  range_maxlathe_l        VARCHAR2(32),
  range_maxlathe_l_u      VARCHAR2(32),
  range_maxlathe_d        VARCHAR2(64),
  range_maxlathe_d_u      VARCHAR2(32),
  range_maxbar_d          VARCHAR2(64),
  range_maxbar_d_u        VARCHAR2(32),
  range_top_dis           VARCHAR2(64),
  range_top_dis_u         VARCHAR2(32),
  range_rail_wide         VARCHAR2(64),
  range_rail_wide_u       VARCHAR2(32),
  spindle_chuck_d         VARCHAR2(64),
  spindle_chuck_d_u       VARCHAR2(32),
  spindle_head_style      VARCHAR2(64),
  spindle_head_style_u    VARCHAR2(32),
  spindle_hole_d          VARCHAR2(64),
  spindle_hole_d_u        VARCHAR2(32),
  spindle_bearing_d       VARCHAR2(64),
  spindle_bearing_d_u     VARCHAR2(32),
  spindle_speed_range     VARCHAR2(64),
  spindle_speed_range_u   VARCHAR2(32),
  spindle_motor_power     VARCHAR2(64),
  spindle_motor_power_u   VARCHAR2(32),
  speed_spindle           VARCHAR2(64),
  spindle_speed_u         VARCHAR2(32),
  spindle_max_torque      VARCHAR2(64),
  spindle_max_torque_u    VARCHAR2(32),
  tailsock_sleeve_d       VARCHAR2(64),
  tailsock_sleeve_d_u     VARCHAR2(32),
  tailsock_sleeve_taper   VARCHAR2(64),
  tailsock_sleeve_taper_u VARCHAR2(32),
  tailsock_sleeve_dis     VARCHAR2(64),
  tailsock_sleeve_dis_u   VARCHAR2(32),
  saddle_lean_angle       VARCHAR2(64),
  saddle_lean_angle_u     VARCHAR2(32),
  saddle_move_dis         VARCHAR2(64),
  saddle_move_dis_u       VARCHAR2(32),
  saddle_move_speed       VARCHAR2(64),
  saddle_move_speed_u     VARCHAR2(32),
  saddle_sevo_torque      VARCHAR2(64),
  saddle_sevo_torque_u    VARCHAR2(32),
  saddle_screw_d          VARCHAR2(64),
  saddle_screw_d_u        VARCHAR2(32),
  toolrest_pos_num        VARCHAR2(64),
  toolrest_pos_num_u      VARCHAR2(32),
  toolrest_tool_size      VARCHAR2(64),
  toolrest_tool_size_u    VARCHAR2(32),
  toolrest_move_speed     VARCHAR2(64),
  toolrest_move_speed_u   VARCHAR2(32),
  toolrest_x_dis          VARCHAR2(64),
  toolrest_x_dis_u        VARCHAR2(32),
  toolrest_z_dis          VARCHAR2(64),
  toolrest_z_dis_u        VARCHAR2(32),
  feed_x_dis              VARCHAR2(64),
  feed_x_dis_u            VARCHAR2(32),
  feed_y_dis              VARCHAR2(64),
  feed_y_dis_u            VARCHAR2(32),
  feed_z_dis              VARCHAR2(64),
  feed_z_dis_u            VARCHAR2(32),
  feed_axis_p             VARCHAR2(64),
  feed_axis_p_u           VARCHAR2(32),
  otherinfo_mach_p        VARCHAR2(64),
  otherinfo_mach_p_u      VARCHAR2(32),
  otherinfo_mach_v        VARCHAR2(64),
  otherinfo_mach_v_u      VARCHAR2(32),
  otherinfo_mach_kg       VARCHAR2(64),
  otherinfo_mach_kg_u     VARCHAR2(32),
  otherinfo_pack_s        VARCHAR2(64),
  otherinfo_pack_s_u      VARCHAR2(32)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_INFO
  add constraint MACH_NUM primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_INFO
  add constraint MACH_NUM_F foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table MACHINE_LOG_FAMILY
prompt =================================
prompt
create table LM04.MACHINE_LOG_FAMILY
(
  type VARCHAR2(64)
)
tablespace CNC
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

prompt
prompt Creating table MACHINE_LOG_STATISTICS
prompt =====================================
prompt
create table LM04.MACHINE_LOG_STATISTICS
(
  id              NUMBER not null,
  mach_num        VARCHAR2(64) not null,
  statistics_date DATE not null,
  pattern_time    NUMBER,
  poweron_time    NUMBER,
  offline_time    NUMBER,
  run_time        NUMBER,
  alarm_time      NUMBER,
  aidle_time      NUMBER,
  now_time        NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_LOG_STATISTICS
  add constraint STATISTICS_ID primary key (ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_LOG_STATISTICS
  add constraint MACH_NUM_F9 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table ROOM_MACHINE
prompt ===========================
prompt
create table LM04.ROOM_MACHINE
(
  mach_num           VARCHAR2(64) not null,
  ip                 VARCHAR2(16),
  iport              NUMBER,
  itype              NUMBER,
  machine_type       VARCHAR2(32),
  system_name        VARCHAR2(32),
  room_id            VARCHAR2(32),
  machine_name       VARCHAR2(32),
  acquiredata_status NUMBER,
  acquireroom_status NUMBER,
  family             VARCHAR2(128),
  channo             NUMBER,
  id                 VARCHAR2(32)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.ROOM_MACHINE
  add constraint MACH_NUM1 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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

prompt
prompt Creating table MACHINE_LOG_TODAY
prompt ================================
prompt
create table LM04.MACHINE_LOG_TODAY
(
  mach_num            VARCHAR2(64) not null,
  ip                  VARCHAR2(16) not null,
  now_time            NUMBER,
  delta_time          NUMBER,
  task_state          INTEGER,
  interp_state        INTEGER,
  disp_mode           INTEGER,
  day_poweron_time    NUMBER,
  day_run_time        NUMBER,
  day_cut_time        NUMBER,
  total_poweron_time  NUMBER,
  total_run_time      NUMBER,
  total_cut_time      NUMBER,
  host_value1         FLOAT,
  host_value2         FLOAT,
  host_value3         FLOAT,
  host_value4         FLOAT,
  host_value5         FLOAT,
  host_value6         FLOAT,
  ahost_value1        FLOAT,
  ahost_value2        FLOAT,
  ahost_value3        FLOAT,
  ahost_value4        FLOAT,
  ahost_value5        FLOAT,
  ahost_value6        FLOAT,
  dist_togo1          FLOAT,
  dist_togo2          FLOAT,
  dist_togo3          FLOAT,
  dist_togo4          FLOAT,
  dist_togo5          FLOAT,
  dist_togo6          FLOAT,
  torq1               FLOAT,
  torq2               FLOAT,
  torq3               FLOAT,
  torq4               FLOAT,
  torq5               FLOAT,
  torq6               FLOAT,
  feed_speed          FLOAT,
  afeed_speed         FLOAT,
  dfeed_speed         FLOAT,
  spindle_speed       FLOAT,
  aspindle_speed      FLOAT,
  dspindle_speed      FLOAT,
  spin_torq           FLOAT,
  travers_scale       FLOAT,
  prog_name           VARCHAR2(128),
  current_line        INTEGER,
  work_piece          INTEGER,
  axis_num            INTEGER,
  tool_num            INTEGER,
  tool_length         FLOAT,
  tool_radius         FLOAT,
  error_id            INTEGER,
  error_axisnum       INTEGER,
  machine_online      INTEGER,
  error_state         INTEGER,
  program_number      INTEGER,
  today_poweron_time  NUMBER,
  today_run_time      NUMBER,
  today_cut_time      NUMBER,
  softversion         VARCHAR2(128),
  today_poweroff_time NUMBER,
  today_idle_time     NUMBER,
  run_time            NUMBER,
  today_error_time    NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_LOG_TODAY
  add constraint MACH_NUM5 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_LOG_TODAY
  add constraint MACHNUM_F_LOGTODAY foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM);

prompt
prompt Creating table MACHINE_MISSION
prompt ==============================
prompt
create table LM04.MACHINE_MISSION
(
  mission             VARCHAR2(128),
  mission_name        VARCHAR2(64),
  room_id             VARCHAR2(64),
  machine_num         VARCHAR2(64),
  machine_name        VARCHAR2(64),
  machine             VARCHAR2(32),
  workpiece_num       NUMBER,
  done                NUMBER,
  undone              NUMBER,
  total               NUMBER,
  log_start           NUMBER,
  log_end             NUMBER,
  actual_start        NUMBER,
  actual_end          NUMBER,
  status              NUMBER,
  rate                VARCHAR2(16),
  man                 VARCHAR2(32),
  mission_description VARCHAR2(512),
  gram                VARCHAR2(64)
)
tablespace CNC
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

prompt
prompt Creating table MACHINE_PICTURE
prompt ==============================
prompt
create table LM04.MACHINE_PICTURE
(
  mach_num                   VARCHAR2(64) not null,
  picture_left               NUMBER,
  picture_top                NUMBER,
  picture_run                VARCHAR2(128),
  picture_stop               VARCHAR2(128),
  picture_alarm_offline_lack VARCHAR2(128),
  picture_low                VARCHAR2(128),
  picture_high               VARCHAR2(128),
  aleft                      NUMBER,
  top                        NUMBER,
  awidth                     NUMBER,
  height                     NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_PICTURE
  add constraint MACH_NUM2 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_PICTURE
  add constraint MACHNUM_F_PICTURE foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM);

prompt
prompt Creating table MACHINE_PROCESS
prompt ==============================
prompt
create table LM04.MACHINE_PROCESS
(
  mach_num   VARCHAR2(64) not null,
  material   NUMBER,
  artificial NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_PROCESS
  add constraint MACH_NUM3 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_PROCESS
  add constraint MACHNUM_F_PROCESS foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM);

prompt
prompt Creating table MACHINE_RECORD
prompt =============================
prompt
create table LM04.MACHINE_RECORD
(
  recordid     NUMBER not null,
  mach_num     VARCHAR2(64) not null,
  modifyuserid VARCHAR2(32),
  modifydate   DATE,
  ip           VARCHAR2(16),
  iport        NUMBER,
  itype        NUMBER,
  machine_type VARCHAR2(32),
  system_name  VARCHAR2(32),
  room_id      VARCHAR2(32),
  machine_name VARCHAR2(32),
  family       VARCHAR2(128),
  channo       NUMBER,
  id           VARCHAR2(32)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_RECORD
  add constraint RECORD_ID primary key (RECORDID)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_RECORD
  add constraint MACH_NUM_F11 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table MACHINE_SPEEDALARM
prompt =================================
prompt
create table LM04.MACHINE_SPEEDALARM
(
  id         NUMBER not null,
  mach_num   VARCHAR2(64) not null,
  status     NUMBER,
  start_time NUMBER,
  end_time   NUMBER,
  last_time  NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.MACHINE_SPEEDALARM
  add constraint SPEEDALARM_ID primary key (ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.MACHINE_SPEEDALARM
  add constraint MACH_NUM10 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE_HISTORY (MACH_NUM);

prompt
prompt Creating table MACHSTATUS_1
prompt ===========================
prompt
create table LM04.MACHSTATUS_1
(
  mach_num    VARCHAR2(64),
  mach_status NUMBER,
  start_time  DATE,
  end_time    DATE,
  flag        NUMBER
)
tablespace CNC
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

prompt
prompt Creating table MACHSTATUS_GJ
prompt ============================
prompt
create table LM04.MACHSTATUS_GJ
(
  mach_num    VARCHAR2(64),
  mach_status NUMBER,
  start_time  DATE,
  end_time    DATE,
  flag        NUMBER
)
tablespace CNC
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

prompt
prompt Creating table PEOPLE_INFO
prompt ==========================
prompt
create table LM04.PEOPLE_INFO
(
  id        VARCHAR2(32) not null,
  room_id   VARCHAR2(128),
  man       VARCHAR2(32),
  sex       VARCHAR2(4),
  position  VARCHAR2(32),
  phone     VARCHAR2(11),
  picture   VARCHAR2(128),
  groupname VARCHAR2(64),
  card      VARCHAR2(4)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.PEOPLE_INFO
  add constraint PEOPLE_ID primary key (ID)
  using index 
  tablespace CNC
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

prompt
prompt Creating table POW_HISTORY_1
prompt ============================
prompt
create table LM04.POW_HISTORY_1
(
  now_time DATE,
  uab      NUMBER,
  ubc      NUMBER,
  uca      NUMBER,
  ia       NUMBER,
  ib       NUMBER,
  ic       NUMBER,
  psum     NUMBER,
  qsum     NUMBER,
  pfsum    NUMBER,
  ssum     NUMBER,
  freq     NUMBER,
  epp      NUMBER,
  epm      NUMBER,
  eqp      NUMBER,
  eqm      NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );

prompt
prompt Creating table POW_LOG_TODAY
prompt ============================
prompt
create table LM04.POW_LOG_TODAY
(
  device_num  NUMBER,
  device_name VARCHAR2(20),
  now_time    DATE,
  uab         NUMBER,
  ubc         NUMBER,
  uca         NUMBER,
  ia          NUMBER,
  ib          NUMBER,
  ic          NUMBER,
  psum        NUMBER,
  qsum        NUMBER,
  pfsum       NUMBER,
  ssum        NUMBER,
  freq        NUMBER,
  epp         NUMBER,
  epm         NUMBER,
  eqp         NUMBER,
  eqm         NUMBER
)
tablespace CNC
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

prompt
prompt Creating table PROGLOG_1
prompt ========================
prompt
create table LM04.PROGLOG_1
(
  mach_num   VARCHAR2(64),
  progname   VARCHAR2(256),
  start_time DATE,
  end_time   DATE,
  flag       NUMBER
)
tablespace CNC
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
comment on column LM04.PROGLOG_1.flag
  is '0开始1停止';

prompt
prompt Creating table RUN_SET_1
prompt ========================
prompt
create table LM04.RUN_SET_1
(
  torq_min       NUMBER,
  torq_max       NUMBER,
  spindle_min    NUMBER,
  spindle_max    NUMBER,
  feed_min       NUMBER,
  feed_max       NUMBER,
  program_exist  NUMBER,
  m_dspindle_min NUMBER,
  m_dspindle_max NUMBER,
  m_dfeed_min    NUMBER,
  m_dfeed_max    NUMBER,
  spindle_zone   NUMBER,
  feed_zone      NUMBER,
  torq_zone      NUMBER,
  efficiency     NUMBER
)
tablespace CNC
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

prompt
prompt Creating table SENSORMACH_CONFIGURE
prompt ===================================
prompt
create table LM04.SENSORMACH_CONFIGURE
(
  mach_num      VARCHAR2(64) not null,
  red           NUMBER,
  yellow        NUMBER,
  green         NUMBER,
  estop         NUMBER,
  spindlea      FLOAT,
  loadthreshold FLOAT,
  expression    VARCHAR2(128)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.SENSORMACH_CONFIGURE
  add constraint MACH_NUM4 primary key (MACH_NUM)
  using index 
  tablespace SYSTEM
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
alter table LM04.SENSORMACH_CONFIGURE
  add constraint MACH_NUM_F3 foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM) on delete cascade;

prompt
prompt Creating table SPEEDALARM_1
prompt ===========================
prompt
create table LM04.SPEEDALARM_1
(
  mach_num    VARCHAR2(64) not null,
  min_dfeed   NUMBER,
  max_dfeed   NUMBER,
  min_spindle NUMBER,
  max_spindle NUMBER,
  min_load    NUMBER,
  max_load    NUMBER,
  afeed       NUMBER,
  dfeed       NUMBER,
  aspindle    NUMBER,
  dspindle    NUMBER,
  load        NUMBER,
  start_time  DATE,
  end_time    DATE,
  lasttime    NUMBER
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );

prompt
prompt Creating table SPEED_ALARM
prompt ==========================
prompt
create table LM04.SPEED_ALARM
(
  slarge   NUMBER,
  ssmall   NUMBER,
  flarge   NUMBER,
  fsmall   NUMBER,
  mach_num VARCHAR2(64) not null
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.SPEED_ALARM
  add constraint MACH_NUM12 primary key (MACH_NUM)
  using index 
  tablespace CNC
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
alter table LM04.SPEED_ALARM
  add constraint MACHNUM_F_SPEEDALARM foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM);

prompt
prompt Creating table STATUS_1
prompt =======================
prompt
create table LM04.STATUS_1
(
  start_time  NUMBER,
  mach_status NUMBER,
  prg_name    VARCHAR2(64),
  error_id    VARCHAR2(32),
  end_time    NUMBER
)
tablespace CNC
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

prompt
prompt Creating table SYSTEM_INFO
prompt ==========================
prompt
create table LM04.SYSTEM_INFO
(
  name               VARCHAR2(20),
  distinguishability VARCHAR2(20),
  max_axis           NUMBER,
  norm_axis          NUMBER,
  cache              NUMBER,
  disk               NUMBER,
  max_disk           NUMBER,
  interpolation      VARCHAR2(100),
  ni                 VARCHAR2(20),
  adjustment         VARCHAR2(20),
  jog                VARCHAR2(20),
  max_speed          VARCHAR2(20),
  spindle_adjustment VARCHAR2(20),
  assist             VARCHAR2(100),
  max_tool           NUMBER,
  max_io             VARCHAR2(20),
  picture            VARCHAR2(100),
  manufacturer       VARCHAR2(100)
)
tablespace CNC
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

prompt
prompt Creating table SYSTEM_RECORD
prompt ============================
prompt
create table LM04.SYSTEM_RECORD
(
  system_lastendtime  DATE,
  system_nowbegintime DATE
)
tablespace CNC
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

prompt
prompt Creating table TEST
prompt ===================
prompt
create table LM04.TEST
(
  id      NUMBER,
  name    VARCHAR2(10),
  sex     VARCHAR2(4),
  age     NUMBER,
  address VARCHAR2(200)
)
tablespace CNC
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

prompt
prompt Creating table USERLOGIN
prompt ========================
prompt
create table LM04.USERLOGIN
(
  user_id  VARCHAR2(32) not null,
  password VARCHAR2(32) not null
)
tablespace CNC
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

prompt
prompt Creating table WORKPIECE
prompt ========================
prompt
create table LM04.WORKPIECE
(
  workpiece_num VARCHAR2(20),
  name          VARCHAR2(20),
  asize         VARCHAR2(100),
  material      VARCHAR2(20),
  surfaceness   VARCHAR2(20),
  description   VARCHAR2(2000),
  picture       VARCHAR2(100),
  pagename      VARCHAR2(100),
  pagefile      VARCHAR2(100)
)
tablespace CNC
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

prompt
prompt Creating table WORKSHOP
prompt =======================
prompt
create table LM04.WORKSHOP
(
  id            NUMBER not null,
  workshop_name VARCHAR2(64),
  unit_name     VARCHAR2(64),
  section_name  VARCHAR2(64),
  mach_num      VARCHAR2(64)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );
alter table LM04.WORKSHOP
  add constraint WORKSHOP_ID primary key (ID)
  using index 
  tablespace SYSTEM
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
alter table LM04.WORKSHOP
  add constraint MACHNUM_F_WORKSHOP foreign key (MACH_NUM)
  references LM04.ROOM_MACHINE (MACH_NUM);

prompt
prompt Creating table WORKTIME
prompt =======================
prompt
create table LM04.WORKTIME
(
  mach_num     VARCHAR2(64),
  userid       VARCHAR2(32),
  poweron_time NUMBER,
  run_time     NUMBER,
  cut_time     NUMBER,
  insert_time  DATE
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );

prompt
prompt Creating table WORKTIME_FLAG
prompt ============================
prompt
create table LM04.WORKTIME_FLAG
(
  mach_num VARCHAR2(64),
  flag     NUMBER
)
tablespace CNC
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

prompt
prompt Creating table WORKTIME_LOGINRECORD
prompt ===================================
prompt
create table LM04.WORKTIME_LOGINRECORD
(
  mach_num   VARCHAR2(32),
  id         VARCHAR2(32),
  start_time DATE,
  end_time   DATE,
  flag       VARCHAR2(32)
)
tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  );

prompt
prompt Creating sequence SEQ_BASDEVICECALENDAR_ID
prompt ==========================================
prompt
create sequence LM04.SEQ_BASDEVICECALENDAR_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_BASSHIFPATTERNLINE_ID
prompt ===========================================
prompt
create sequence LM04.SEQ_BASSHIFPATTERNLINE_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_ERROR_LOG_ID
prompt ==================================
prompt
create sequence LM04.SEQ_ERROR_LOG_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 8080
increment by 1
cache 20;

prompt
prompt Creating sequence SEQ_FIX_ID
prompt ============================
prompt
create sequence LM04.SEQ_FIX_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_FIX_SYS_ID
prompt ================================
prompt
create sequence LM04.SEQ_FIX_SYS_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_LOOK_HISTORY_ID
prompt =====================================
prompt
create sequence LM04.SEQ_LOOK_HISTORY_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_MACHINE_RECORD_ID
prompt =======================================
prompt
create sequence LM04.SEQ_MACHINE_RECORD_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 134
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_SPEEDALARM_ID
prompt ===================================
prompt
create sequence LM04.SEQ_SPEEDALARM_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_STATISTICS_ID
prompt ===================================
prompt
create sequence LM04.SEQ_STATISTICS_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 2
increment by 1
nocache;

prompt
prompt Creating sequence SEQ_WORKSHOP_ID
prompt =================================
prompt
create sequence LM04.SEQ_WORKSHOP_ID
minvalue 1
maxvalue 9999999999999999999999999999
start with 157
increment by 1
nocache;

prompt
prompt Creating procedure PROC1
prompt ========================
prompt
create or replace procedure lm04.proc1
is
begin insert into test(ID,NAME,SEX,AGE) values
(1,'moses','man',25);
commit;
end;
/

prompt
prompt Creating procedure PROC2
prompt ========================
prompt
create or replace procedure lm04.proc2(v_id  number,
v_name varchar2) is
begin insert into test(id,name)
  values(v_id,v_name);
  commit;
end proc2;
/

prompt
prompt Creating procedure PRO_HISTORYBYMACHNUMDELETE
prompt =============================================
prompt
create or replace procedure lm04.PRO_HISTORYBYMACHNUMDELETE(v_machnum in varchar2,v_msg out number) is
v_flag NUMBER;--标识该机床是否存在
my_machnumnoexist_exp EXCEPTION;
PRAGMA EXCEPTION_INIT(my_machnumnoexist_exp,-20006);--机床编号不存在异常
v_tablename varchar2(100);--拼接表名
--v_sequence varchar2(100);--拼接sequence
v_sql varchar2(2000);--定义动态表名
v_section_name varchar2(64);--存储工段名
v_unit_name varchar2(64);--存储单元名
v_workshop_name varchar2(64);--存储车间名

v_flag_table number;--判断建表异常在哪一步
begin
  select count(mach_num) into v_flag from room_machine_history where mach_num=v_machnum;
  if v_flag=0 then
     raise my_machnumnoexist_exp;
  end if;
  --判断该机床现在是否还在使用
  select count(mach_num) into v_flag from room_machine where mach_num=v_machnum;
  if v_flag>0 then
     v_flag_table:=8;
     v_tablename:='error_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

    /* v_flag_table:=7;
     v_sequence:='seq_error_'||v_machnum||'_id';
     v_sql:='drop sequence '||v_sequence;
     execute immediate v_sql;*/

     v_flag_table:=6;
     v_tablename:='speedalarm_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

    /* v_flag_table:=5;
     v_sequence:='seq_speedalarm_'||v_machnum||'_id';
     v_sql:='drop sequence '||v_sequence;
     execute immediate v_sql;*/

     v_flag_table:=4;
     v_tablename:='error_set_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

     v_flag_table:=3;
     v_tablename:='run_set_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

     v_flag_table:=2;
     v_tablename:='status_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

     v_flag_table:=1;
     v_tablename:='rstatus_'||v_machnum;
     v_sql:='drop table '||v_tablename;
     execute immediate v_sql;

     v_flag_table:=0;
     delete from machine_picture where mach_num=v_machnum;
     delete from machine_process where mach_num=v_machnum;
     delete from sensormach_configure where mach_num=v_machnum;
     delete from machine_log_today where mach_num=v_machnum;
     delete from speed_alarm where mach_num = v_machnum;
     select count(mach_num) into v_flag from workshop where mach_num=v_machnum;
   if v_flag>0 then
     select section_name into v_section_name from workshop where mach_num=v_machnum;
     select unit_name into v_unit_name from workshop where mach_num=v_machnum;
     select workshop_name into v_workshop_name from workshop where mach_num=v_machnum;
     select count(section_name) into v_flag from workshop where section_name=v_section_name and unit_name=v_unit_name and workshop_name=v_workshop_name;
     if v_flag=1 then
       update workshop set mach_num=null where mach_num=v_machnum;
     end if;
     if v_flag>1 then
        delete from workshop where mach_num=v_machnum;
     end if;
   end if;
     delete from room_machine where mach_num=v_machnum;
   end if;

  delete from machine_record where mach_num=v_machnum;
  delete from machine_log_statistics where mach_num=v_machnum;
  delete from fix_record where mach_num=v_machnum;
  delete from fix_record_sys where mach_num=v_machnum;
  delete from error where mach_num=v_machnum;
  delete from machine_speedalarm where mach_num=v_machnum;
  delete from machine_info where mach_num=v_machnum;
  delete from room_machine_history where mach_num=v_machnum;
  commit;
  v_msg:=1;
  exception
    when my_machnumnoexist_exp then
      rollback;
      v_msg:=2;
    when others then
      rollback;
      if(v_flag_table<8) then
        v_tablename:='error_'||v_machnum;
        v_sql:='create table '||v_tablename||'
        (
          id          NUMBER PRIMARY KEY,
          errorid     NUMBER,
          type        NUMBER,
          starttime   DATE,
          endtime     DATE,
          errorinfo   VARCHAR2(256),
          description VARCHAR2(128)
        )
          tablespace CNC
          pctfree 10
          initrans 1
          maxtrans 255
          storage
       (
         initial 64K
         next 8K
         minextents 1
         maxextents unlimited
        )';
       execute immediate v_sql;
      /* if(v_flag_table<7) then
          v_sequence:='seq_error_'||v_machnum||'_id';
          v_sql:='create sequence '||v_sequence;
          execute immediate v_sql;*/
          if(v_flag_table<6) then
             v_tablename:='speedalarm_'||v_machnum;
             v_sql:='create table '||v_tablename||'
             (
               id         NUMBER PRIMARY KEY,
               mach_num   VARCHAR2(64) not null,
               status     NUMBER,
               start_time NUMBER,
               end_time   NUMBER,
               last_time  NUMBER
               )
               tablespace CNC
               pctfree 10
               initrans 1
               maxtrans 255
               storage
               (
                 initial 64K
                 next 8K
                 minextents 1
                 maxextents unlimited
               )';
              execute immediate v_sql;
             /* if(v_flag_table<5) then
                 v_sequence:='seq_speedalarm_'||v_machnum||'_id';
                 v_sql:='create sequence '||v_sequence;
                 execute immediate v_sql;*/
                 if(v_flag_table<4) then
                    v_tablename:='error_set_'||v_machnum;
                    v_sql:='create table '||v_tablename||'
                    (
                      error_id VARCHAR2(32)
                    )
                    tablespace CNC
                    pctfree 10
                    initrans 1
                    maxtrans 255
                    storage
                    (
                      initial 64K
                      next 1M
                      minextents 1
                      maxextents unlimited
                    )';
                    execute immediate v_sql;
                    if(v_flag_table<3) then
                      v_tablename:='run_set_'||v_machnum;
                      v_sql:='create table '||v_tablename||'
                      (
                      torq_min       NUMBER,
                      torq_max       NUMBER,
                      spindle_min    NUMBER,
                      spindle_max    NUMBER,
                      feed_min       NUMBER,
                      feed_max       NUMBER,
                      program_exist  NUMBER,
                      m_dspindle_min NUMBER,
                      m_dspindle_max NUMBER,
                      m_dfeed_min    NUMBER,
                      m_dfeed_max    NUMBER,
                      spindle_zone   NUMBER,
                      feed_zone      NUMBER,
                      torq_zone      NUMBER,
                      efficiency     NUMBER
                      )
                      tablespace CNC
                      pctfree 10
                      initrans 1
                      maxtrans 255
                      storage
                      (
                        initial 64K
                        next 1M
                        minextents 1
                        maxextents unlimited
                      )';
                      execute immediate v_sql;
                      if(v_flag_table<2) then
                         v_tablename:='status_'||v_machnum;
                         v_sql:='create table '||v_tablename||'
                         (
                         start_time  NUMBER,
                         mach_status NUMBER,
                         prg_name    VARCHAR2(64),
                         error_id    VARCHAR2(32),
                         end_time    NUMBER
                         )
                         tablespace CNC
                         pctfree 10
                         initrans 1
                         maxtrans 255
                         storage
                         (
                          initial 64K
                          next 1M
                          minextents 1
                          maxextents unlimited
                          )';
                          execute immediate v_sql;
                          if(v_flag_table<1) then
                             v_tablename:='rstatus_'||v_machnum;
                             v_sql:='create table '||v_tablename||'
                             (
                             start_time          NUMBER,
                             end_time            NUMBER,
                             mach_status         NUMBER,
                             auto_error_ret_code NUMBER,
                             plc_error_ret_code  NUMBER,
                             plc_errorid         VARCHAR2(32),
                             prg_name            VARCHAR2(64),
                             auto_errorid        NUMBER
                             )
                             tablespace CNC
                             pctfree 10
                             initrans 1
                             maxtrans 255
                             storage
                             (
                              initial 64K
                              next 1M
                              minextents 1
                              maxextents unlimited
                            )';
                            execute immediate v_sql;
                         end if;
                   end if;
                 end if;
              end if;
            end if;
          end if;
   --    end if;
  -- end if;
      v_msg:=3;
end PRO_HISTORYBYMACHNUMDELETE;
/

prompt
prompt Creating procedure PRO_MACHINEIOFOCOLLECT
prompt =========================================
prompt
create or replace procedure lm04.PRO_MACHINEIOFOCOLLECT is
  v_flag       number; --记录表中是否有数据
  machnum      varchar2(64); --接收机床编号
  machnumtable varchar2(64); --拼接故障表名
  --v_sequence   varchar2(64); --拼接sequence
  v_sql        varchar2(2000); --定义动态表名
begin
  declare
    cursor cur is
      select mach_num from room_machine;
  begin
    for x in cur loop
      machnum      := x.mach_num;
      machnumtable := 'error_' || machnum;
      v_sql        := 'select count(*) from ' || machnumtable;
      execute immediate v_sql
        into v_flag;
      if v_flag > 0 then
        execute immediate ' insert into error（mach_num,error_id,error_type,start_time,end_time,last_time,date_zone,error_info,description）
       select :1,errorid,type,starttime,endtime,(endtime-starttime)*24*60*60,trunc(starttime),errorinfo,description from ' || machnumtable
          using machnum;
        v_sql := 'truncate table ' || machnumtable;
        execute immediate v_sql;
        /*v_sequence := 'seq_error_' || machnum || '_id';
        v_sql      := 'drop sequence ' || v_sequence;
        execute immediate v_sql;
        v_sequence := 'seq_error_' || machnum || '_id';
        v_sql      := 'create sequence ' || v_sequence;
        execute immediate v_sql;*/
      end if;
     /* machnumtable := 'speedalarm_' || machnum;
      v_sql        := 'select count(*) from ' || machnumtable;
      execute immediate v_sql
        into v_flag;
      if v_flag > 0 then
        execute immediate ' insert into machine_speedalarm（mach_num,status,start_time,end_time,last_time）
            select :1,status,start_time,end_time,last_time from ' ||
                          machnumtable
          using machnum;
        v_sql := 'truncate table ' || machnumtable;
        execute immediate v_sql;*/
        /*v_sequence := 'seq_speedalarm_' || machnum || '_id';
        v_sql      := 'drop sequence ' || v_sequence;
        execute immediate v_sql;
        v_sequence := 'seq_speedalarm_' || machnum || '_id';
        v_sql      := 'create sequence ' || v_sequence;
        execute immediate v_sql;*/
     -- end if;
    end loop;
  end;
  commit;
exception
  when others then
    rollback;
end PRO_MACHINEIOFOCOLLECT;
/

prompt
prompt Creating procedure PRO_PEOPLEINFOBYIDDELETE
prompt ===========================================
prompt
create or replace procedure lm04.PRO_PEOPLEINFOBYIDDELETE(v_id  in varchar2,
                                                     v_msg out number) is
  v_flag number;
  my_peoplenoexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_peoplenoexist_exp, -20001); --人员名称不存在
  my_02292_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_02292_exp, -02292); --外键异常
begin
  select count(id) into v_flag from people_info where id = v_id;
  if v_flag = 0 then
    raise my_peoplenoexist_exp;
  end if;
  delete from dncuser where username = v_id;
  delete from people_info where id = v_id;
  commit;
  v_msg := 1;
exception
  when my_peoplenoexist_exp then
    rollback;
    v_msg := 2;
  when my_02292_exp then
    rollback;
    v_msg := 3;
  when others then
    rollback;
    v_msg := 4;
end PRO_PEOPLEINFOBYIDDELETE;
/

prompt
prompt Creating procedure PRO_PEOPLEINFOBYIDINSERT
prompt ===========================================
prompt
create or replace procedure lm04.PRO_PEOPLEINFOBYIDINSERT(v_id        in varchar2,
                                                     v_roomid    in varchar2,
                                                     v_man       in varchar2,
                                                     v_sex       in varchar2,
                                                     v_position  in varchar2,
                                                     v_phone     in number,
                                                     v_picture   in varchar2,
                                                     v_passwd    in varchar2,
                                                     v_authority in number,
                                                     v_card      in varchar2,
                                                     v_msg       out number) is
begin
  insert into people_info
    (id, room_id, man, sex, position, phone, picture,card)
  values
    (v_id, v_roomid, v_man, v_sex, v_position, v_phone, v_picture,v_card);
  insert into dncuser
    (username, passwd, authority)
  values
    (v_id, v_passwd, v_authority);
  commit;
  v_msg := 1;
exception
  when Dup_val_on_index then
    rollback;
    v_msg := 2;
  when others then
    rollback;
    v_msg := 3;
end PRO_PEOPLEINFOBYIDINSERT;
/

prompt
prompt Creating procedure PRO_PEOPLEINFOBYIDUPDATE
prompt ===========================================
prompt
CREATE OR REPLACE PROCEDURE LM04.PRO_PEOPLEINFOBYIDUPDATE(V_ID        IN VARCHAR2,
                                                     V_ROOMID    IN VARCHAR2,
                                                     V_MAN       IN VARCHAR2,
                                                     V_SEX       IN VARCHAR2,
                                                     V_POSITION  IN VARCHAR2,
                                                     V_PHONE     IN NUMBER,
                                                     V_PICTURE   IN VARCHAR2,
                                                     V_PASSWD    IN VARCHAR2,
                                                     V_AUTHORITY IN NUMBER,
                                                     V_MSG       OUT NUMBER) IS
BEGIN
  UPDATE PEOPLE_INFO
     SET ROOM_ID  = V_ROOMID,
         MAN      = V_MAN,
         SEX      = V_SEX,
         POSITION = V_POSITION,
         PHONE    = V_PHONE,
         PICTURE  = V_PICTURE
   WHERE ID = V_ID;

IF V_PASSWD IS NULL THEN
  UPDATE DNCUSER
     SET  AUTHORITY = V_AUTHORITY
   WHERE USERNAME = V_ID;
ELSE
   UPDATE DNCUSER
     SET PASSWD = V_PASSWD, AUTHORITY = V_AUTHORITY
   WHERE USERNAME = V_ID;
 END IF;



  COMMIT;
  V_MSG := 1;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    V_MSG := 2;
END PRO_PEOPLEINFOBYIDUPDATE;
/

prompt
prompt Creating procedure PRO_ROOMMACHINEBYMACHNUMDELETE
prompt =================================================
prompt
create or replace procedure lm04.PRO_ROOMMACHINEBYMACHNUMDELETE(v_machnum  in varchar2,
                                                           v_removeid in varchar2,
                                                           v_msg      out number) is
  v_flag NUMBER; --标识该机床是否存在
  my_machnumnoexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_machnumnoexist_exp, -20004); --机床编号不存在异常
  my_useridnoexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_useridnoexist_exp, -20002); --用户编号不存在异常
  v_tablename    varchar2(100); --拼接表名
  --v_sequence     varchar2(100); --拼接sequence
  v_sql          varchar2(2000); --定义动态表名
  v_uppermachnum varchar2(64); --存储大写机床编号
  v_tabext       number; --判断表是否已经存在
--  v_seqext       number; --判断序列号是否已经存在
  v_section_name varchar2(64);--存储工段名
  v_unit_name varchar2(64);--存储单元名
  v_workshop_name varchar2(64);--存储车间名
  v_flag_table number; --判断建表异常在哪一步
begin
  /*
  *此部分用于查询用户异常
  */
  --判断要删除的机床在room_machine中是否存在，如果不存在，捕获异常
  select count(mach_num) into v_flag from room_machine where mach_num = v_machnum;
  if v_flag = 0 then
    raise my_machnumnoexist_exp;
  end if;
  --判断操作者在people_info中是否存在，如果不存在，捕获异常
  select count(id) into v_flag from people_info where id = v_removeid;
  if v_flag = 0 then
    raise my_useridnoexist_exp;
  end if;
  /*
  *此进行表的删除操作
  */
  v_uppermachnum := upper(v_machnum);
  v_flag_table := 9;
  --将PROGLOG_机床编号_ID序列号删除
  v_tablename := 'PROGLOG_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table   := 8;
  --将MACHINE_ERROR_机床编号里的信息汇集并删除该表
  v_tablename    := 'ERROR_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'select count(*) from ' || v_tablename;
    execute immediate v_sql into v_flag;
    if v_flag > 0 then
      execute immediate 'insert into error（mach_num,error_id,error_type,start_time,end_time,last_time,date_zone,error_info,description）
     select :1,errorid,type,starttime,endtime,(endtime-starttime)*24*60*60,trunc(starttime),errorinfo,description from ' ||v_tablename
        using v_machnum;
    end if;
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 7;
  --将MACHINE_ERROR_机床编号_ID序列号删除
  /*v_sequence := 'SEQ_ERROR_' || v_uppermachnum || '_ID';
  select count(1) into v_seqext from user_sequences where sequence_name = v_sequence;
  if v_seqext = 1 then
    v_sql := 'drop sequence ' || v_sequence;
    execute immediate v_sql;
  end if;*/
  v_flag_table := 6;
  --将MACHINE_SPEEDALARM_机床编号表里的信息汇集并删除该表
  v_tablename := 'SPEEDALARM_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    if v_flag > 0 then
      execute immediate ' insert into machine_speedalarm（mach_num,status,start_time,end_time,last_time）
      select :1,status,start_time,end_time,last_time from ' || v_tablename
      using v_machnum;
    end if;
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 5;
  --将POW_HISTORY_机床编号_ID序列号删除
  v_tablename := 'POW_HISTORY_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 4;
  --将MACHINE_ERROR_SET_机床编号表删除
  v_tablename := 'ERROR_SET_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 3;
  --将MACHINE_RUN_SET_机床编号表删除
  v_tablename := 'RUN_SET_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 2;
  --将MACHINE_STATUS_机床编号表删除
  v_tablename := 'STATUS_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 1;
  --将REAL_MACHINE_STATUS_机床编号表删除
  v_tablename := 'MACHSTATUS_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_sql := 'drop table ' || v_tablename;
    execute immediate v_sql;
  end if;
  v_flag_table := 0;
  update room_machine_history set removeuserid = v_removeid, removedate = trunc(sysdate) where mach_num = v_machnum;
  delete from machine_picture where mach_num = v_machnum;
  delete from machine_process where mach_num = v_machnum;
  delete from sensormach_configure where mach_num = v_machnum;
  delete from machine_log_today where mach_num = v_machnum;
  delete from speed_alarm where mach_num = v_machnum;
  delete from pow_log_today where device_num = v_machnum;
  select count(mach_num) into v_flag from workshop where mach_num=v_machnum;
  if v_flag>0 then
    select section_name into v_section_name from workshop where mach_num=v_machnum;
    select unit_name into v_unit_name from workshop where mach_num=v_machnum;
    select workshop_name into v_workshop_name from workshop where mach_num=v_machnum;
    select count(section_name) into v_flag from workshop where section_name=v_section_name and unit_name=v_unit_name and workshop_name=v_workshop_name;
    if v_flag=1 then
      update workshop set mach_num=null where mach_num=v_machnum;
    end if;
    if v_flag>1 then
      delete from workshop where mach_num=v_machnum;
    end if;
  end if;
  delete from room_machine where mach_num = v_machnum;
  commit;
  v_msg := 1;
 /*
 *此部分为异常处理部分
 */
exception
  when my_machnumnoexist_exp then
    rollback;
    v_msg := 2;
  when my_useridnoexist_exp then
    rollback;
    v_msg := 3;
  when others then
    rollback;
    if (v_flag_table < 9) then
      v_tablename := 'proglog_' || v_machnum;
      v_sql       := 'create table ' || v_tablename || '
        (
          MACH_NUM     VARCHAR2(64),
          PROGNAME     VARCHAR2(256),
          START_TIME   DATE,
          END_TIME     DATE,
          FLAG         NUMBER    
        )
          tablespace CNC
          pctfree 10
          initrans 1
          maxtrans 255
          storage
       (
         initial 64K
         next 8K
         minextents 1
         maxextents unlimited
        )';
      execute immediate v_sql;
    if (v_flag_table < 8) then
      v_tablename := 'error_' || v_machnum;
      v_sql       := 'create table ' || v_tablename || '
        (
          errorid     NUMBER,
          type        NUMBER,
          starttime   DATE,
          endtime     DATE,
          errorinfo   VARCHAR2(256),
          description VARCHAR2(128)
        )
          tablespace CNC
          pctfree 10
          initrans 1
          maxtrans 255
          storage
       (
         initial 64K
         next 8K
         minextents 1
         maxextents unlimited
        )';
      execute immediate v_sql;
      if (v_flag_table < 7) then
       /* v_sequence := 'seq_error_' || v_machnum || '_id';
        v_sql      := 'create sequence ' || v_sequence;
        execute immediate v_sql;*/
        if (v_flag_table < 6) then
          v_tablename := 'speedalarm_' || v_machnum;
          v_sql       := 'create table ' || v_tablename || '
             (
               mach_num   VARCHAR2(64) not null,
               status     NUMBER,
               start_time NUMBER,
               end_time   NUMBER,
               last_time  NUMBER
               )
               tablespace CNC
               pctfree 10
               initrans 1
               maxtrans 255
               storage
               (
                 initial 64K
                 next 8K
                 minextents 1
                 maxextents unlimited
               )';
          execute immediate v_sql;
          if (v_flag_table < 5) then
        v_tablename := 'pow_history_' || v_machnum;
          v_sql       := 'create table ' || v_tablename || '
             (
               now_time DATE,
               uab      NUMBER,
               ubc      NUMBER,
               uca      NUMBER,
               ia       NUMBER,
               ib       NUMBER,
               ic       NUMBER,
               psum     NUMBER,
               qsum     NUMBER,
               pfsum    NUMBER,
               ssum     NUMBER,
               freq     NUMBER,
               epp      NUMBER,
               epm      NUMBER,
               eqp      NUMBER,
               eqm      NUMBER
               )
               tablespace CNC
               pctfree 10
               initrans 1
               maxtrans 255
               storage
               (
                 initial 64K
                 next 8K
                 minextents 1
                 maxextents unlimited
               )';
            if (v_flag_table < 4) then
              v_tablename := 'error_set_' || v_machnum;
              v_sql       := 'create table ' || v_tablename || '
                    (
                      error_id VARCHAR2(32)
                    )
                    tablespace CNC
                    pctfree 10
                    initrans 1
                    maxtrans 255
                    storage
                    (
                      initial 64K
                      next 1M
                      minextents 1
                      maxextents unlimited
                    )';
              execute immediate v_sql;
              if (v_flag_table < 3) then
                v_tablename := 'run_set_' || v_machnum;
                v_sql       := 'create table ' || v_tablename || '
                      (
                      torq_min       NUMBER,
                      torq_max       NUMBER,
                      spindle_min    NUMBER,
                      spindle_max    NUMBER,
                      feed_min       NUMBER,
                      feed_max       NUMBER,
                      program_exist  NUMBER,
                      m_dspindle_min NUMBER,
                      m_dspindle_max NUMBER,
                      m_dfeed_min    NUMBER,
                      m_dfeed_max    NUMBER,
                      spindle_zone   NUMBER,
                      feed_zone      NUMBER,
                      torq_zone      NUMBER,
                      efficiency     NUMBER
                      )
                      tablespace CNC
                      pctfree 10
                      initrans 1
                      maxtrans 255
                      storage
                      (
                        initial 64K
                        next 1M
                        minextents 1
                        maxextents unlimited
                      )';
                execute immediate v_sql;
                if (v_flag_table < 2) then
                  v_tablename := 'status_' || v_machnum;
                  v_sql       := 'create table ' || v_tablename || '
                         (
                         start_time  NUMBER,
                         mach_status NUMBER,
                         prg_name    VARCHAR2(64),
                         error_id    VARCHAR2(32),
                         end_time    NUMBER
                         )
                         tablespace CNC
                         pctfree 10
                         initrans 1
                         maxtrans 255
                         storage
                         (
                          initial 64K
                          next 1M
                          minextents 1
                          maxextents unlimited
                          )';
                  execute immediate v_sql;
                  if (v_flag_table < 1) then
                    v_tablename := 'rstatus_' || v_machnum;
                    v_sql       := 'create table ' || v_tablename || '
                             (
                             start_time          NUMBER,
                             end_time            NUMBER,
                             mach_status         NUMBER,
                             auto_error_ret_code NUMBER,
                             plc_error_ret_code  NUMBER,
                             plc_errorid         VARCHAR2(32),
                             prg_name            VARCHAR2(64),
                             auto_errorid        NUMBER
                             )
                             tablespace CNC
                             pctfree 10
                             initrans 1
                             maxtrans 255
                             storage
                             (
                              initial 64K
                              next 1M
                              minextents 1
                              maxextents unlimited
                            )';
                    execute immediate v_sql;
                  end if;
                end if;
              end if;
            end if;
          end if;
        end if;
      end if;
    end if;
    end if;
  v_msg := 4;
end PRO_ROOMMACHINEBYMACHNUMDELETE;
/

prompt
prompt Creating procedure PRO_ROOMMACHINEBYMACHNUMINSERT
prompt =================================================
prompt
create or replace procedure lm04.pro_RoomMachineByMachNumInsert(v_machnum                 in varchar2,
                                                           v_createuserid            in varchar2,
                                                           v_ip                      in varchar2,
                                                           v_iport                   in number,
                                                           v_itype                   in number,
                                                           v_machinetype             in varchar2,
                                                           v_systemname              in varchar2,
                                                           v_roomid                  in varchar2,
                                                           v_machinename             in varchar2,
                                                           v_family                  in varchar2,
                                                           v_channo                  in number,
                                                           v_id                      in varchar2,
                                                           v_picturerun              in varchar2,
                                                           v_picturestop             in varchar2,
                                                           v_picturealarmofflinelack in varchar2,
                                                           v_picturelow              in varchar2,
                                                           v_picturehigh             in varchar2,
                                                           v_aleft                   in number,
                                                           v_top                     in number,
                                                           v_awidth                  in number,
                                                           v_height                  in number,
                                                           v_red                     in number,
                                                           v_yellow                  in number,
                                                           v_green                   in number,
                                                           v_estop                   in number,
                                                           v_spindlea                in number,
                                                           v_loadthreshold           in number,
                                                           v_expression              in varchar2,
                                                           v_msg                     out number) is
  my_machnumexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_machnumexist_exp, -20001); --机床已经存在
  my_useridnoexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_useridnoexist_exp, -20002); --用户名不存在
  my_ipexist_exp EXCEPTION;
  PRAGMA EXCEPTION_INIT(my_ipexist_exp, -20003); --ip已经在使用

  v_flag          number; --判断机床编号是否在room_machine_history里存在
  v_tablename     varchar2(100); --拼接表名
 -- v_sequence      varchar2(100); --拼接sequence
  v_tabext        number; --判断表是否已经存在
 -- v_seqext        number; --判断序列号是否已经存在
  v_drop          varchar2(500); --定义删除语句
  v_sql           varchar2(1000); --定义动态表名
  v_uppermachnum  varchar2(64);  --保存大写机床编号，用于查询表是否存在
  v_flag_table    number; --判断建表异常在哪一步

begin
  /*
  *此部分用于用户自定义异常捕获
  */
  --查询机床在room_machine中是否存在，如果存在捕获异常
  select count(mach_num) into v_flag from room_machine where mach_num = v_machnum;
  if v_flag > 0 then
    raise my_machnumexist_exp;
  end if;
  --查询机床管理者在PEOPLE_INFO中是否存在，如果不存在捕获异常
  select count(id) into v_flag from people_info where id = v_id;
  if v_flag = 0 then
    raise my_useridnoexist_exp;
  end if;
  --查询添加机床者在PEOPLE_INFO中是否存在，如果不存在捕获异常
  select count(id) into v_flag from people_info where id = v_createuserid;
  if v_flag = 0 then
    raise my_useridnoexist_exp;
  end if;
  --查询添加机床的IP在room_machine中是否存在，如果存在捕获异常
  select count(ip) into v_flag from room_machine where ip = v_ip;
  if v_flag > 0 then
    raise my_ipexist_exp;
  end if;
  /*
  *此部分用于创建数据表和序列号
  */
  --添加一个叫PROGLOG_机床编号的表
  v_flag_table := 9;
  v_tablename  := 'PROGLOG_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
   (
   MACH_NUM  VARCHAR2(64),
   PROGNAME VARCHAR2(256),
   START_TIME    DATE,
   END_TIME    DATE,
   FLAG    NUMBER
   )
   tablespace CNC
   pctfree 10
   initrans 1
   maxtrans 255
   storage
   (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
    )';
  execute immediate v_sql;
 --添加能耗的历史表
  v_uppermachnum:=upper(v_machnum);
  v_flag_table := 8;
  v_tablename  := 'POW_HISTORY_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
   v_sql := 'create table ' || v_tablename || '
  (
  now_time DATE,
  uab      NUMBER,
  ubc      NUMBER,
  uca      NUMBER,
  ia       NUMBER,
  ib       NUMBER,
  ic       NUMBER,
  psum     NUMBER,
  qsum     NUMBER,
  pfsum    NUMBER,
  ssum     NUMBER,
  freq     NUMBER,
  epp      NUMBER,
  epm      NUMBER,
  eqp      NUMBER,
  eqm      NUMBER
  )
  tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
  initial 64K
  next 8K
  minextents 1
  maxextents unlimited
  )';
  execute immediate v_sql;
   --添加一个叫SEQ_ERROR_机床编号_ID的sequence
 /* v_sequence   := 'SEQ_ERROR_' ||v_uppermachnum|| '_ID';
  select count(1) into v_seqext from user_sequences where sequence_name = v_sequence;
  if v_seqext = 1 then
    v_drop := 'drop sequence ' || v_sequence;
    execute immediate v_drop;
  end if;
  v_sql := 'create sequence ' || v_sequence;
  execute immediate v_sql;*/
  -- 添加一个叫MACHINE_ERROR_机床编号的表
  v_flag_table := 7;
  v_tablename  := 'ERROR_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
  (
  errorid     NUMBER,
  type        NUMBER,
  starttime   DATE,
  endtime     DATE,
  errorinfo   VARCHAR2(256),
  description VARCHAR2(128)
  )
  tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
  initial 64K
  next 8K
  minextents 1
  maxextents unlimited
  )';
  execute immediate v_sql;
  --添加一个叫SEQ_SPEEDALARM_机床编号_ID的sequence
  v_flag_table := 6;
 /* v_sequence   := 'SEQ_SPEEDALARM_' ||v_uppermachnum|| '_ID';
  select count(1) into v_seqext from user_sequences where sequence_name = v_sequence;
  if v_seqext = 1 then
    v_drop := 'drop sequence ' || v_sequence;
    execute immediate v_drop;
  end if;
  v_sql := 'create sequence ' || v_sequence;
  execute immediate v_sql;*/
  -- 添加一个叫MACHINE_SPEEDALARM_机床编号的表
  v_flag_table := 5;
  v_tablename  := 'SPEEDALARM_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
  (
   mach_num   VARCHAR2(64) not null,
   min_dfeed  NUMBER,
   MAX_DFEED  NUMBER,
   MIN_SPINDLE NUMBER,
   MAX_SPINDLE NUMBER,
   MIN_LOAD    NUMBER,
   MAX_LOAD    NUMBER,
   AFEED      NUMBER,
   DFEED      NUMBER,
   ASPINDLE   NUMBER,
   DSPINDLE   NUMBER,
   LOAD       NUMBER,
   START_TIME DATE,
   END_TIME   DATE,
   LASTTIME   NUMBER
  )
  tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 8K
    minextents 1
    maxextents unlimited
  )';
  execute immediate v_sql;
  --添加一个叫MACHINE_ERROR_SET_机床编号的表
  v_flag_table := 4;
  v_tablename  := 'ERROR_SET_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
  (
  error_id VARCHAR2(32)
  )
  tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  )';
  execute immediate v_sql;
  --添加一个叫MACHINE_RUN_SET_机床编号的表
  v_flag_table := 3;
  v_tablename  := 'RUN_SET_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
  (
  torq_min       NUMBER,
  torq_max       NUMBER,
  spindle_min    NUMBER,
  spindle_max    NUMBER,
  feed_min       NUMBER,
  feed_max       NUMBER,
  program_exist  NUMBER,
  m_dspindle_min NUMBER,
  m_dspindle_max NUMBER,
  m_dfeed_min    NUMBER,
  m_dfeed_max    NUMBER,
  spindle_zone   NUMBER,
  feed_zone      NUMBER,
  torq_zone      NUMBER,
  efficiency     NUMBER
  )
  tablespace CNC
  pctfree 10
  initrans 1
  maxtrans 255
  storage
  (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  )';
  execute immediate v_sql;

  v_sql := 'insert into ' || v_tablename || '(TORQ_MIN,TORQ_MAX,SPINDLE_MIN,SPINDLE_MAX,FEED_MIN,FEED_MAX,PROGRAM_EXIST,M_DSPINDLE_MIN,M_DSPINDLE_MAX,M_DFEED_MIN,M_DFEED_MAX,SPINDLE_ZONE,FEED_ZONE,TORQ_ZONE,EFFICIENCY)
                                       values(0,100,0,1000,0,1000,0,0,1000,0,1000,0,0,0,1000)';
  execute immediate v_sql;

  --添加一个叫MACHINE_STATUS_机床编号的表
  v_flag_table := 2;
  v_tablename  := 'STATUS_' ||v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
   (
   start_time  NUMBER,
   mach_status NUMBER,
   prg_name    VARCHAR2(64),
   error_id    VARCHAR2(32),
   end_time    NUMBER
   )
   tablespace CNC
   pctfree 10
   initrans 1
   maxtrans 255
   storage
   (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
    )';
  execute immediate v_sql;
  --添加一个叫MACHSTATUS_机床编号的表
  v_flag_table := 1;
  v_tablename  := 'MACHSTATUS_' || v_uppermachnum;
  select count(1) into v_tabext from user_tables where table_name = v_tablename;
  if v_tabext = 1 then
    v_drop := 'drop table ' || v_tablename;
    execute immediate v_drop;
  end if;
  v_sql := 'create table ' || v_tablename || '
   (
   MACH_NUM           VARCHAR2(64),
   MACH_STATUS        NUMBER,
   START_TIME         DATE,
   END_TIME           DATE,
   FLAG               NUMBER
   )
   tablespace CNC
   pctfree 10
   initrans 1
   maxtrans 255
   storage
   (
    initial 64K
    next 1M
    minextents 1
    maxextents unlimited
  )';
  execute immediate v_sql;
  
  v_flag_table := 0;
 /*
  *此部分用于对表的操作
  */
  --判断添加的机床编号在room_machine_history中是否存在，若存在，将移除时间和移除者清空，若不存在添加机床信息
  select count(mach_num) into v_flag from room_machine_history where mach_num = v_machnum;
  if v_flag = 0 then
    insert into room_machine_history(mach_num,createuserid,createdate,ip,iport,itype,machine_type,system_name,room_id,machine_name,family,channo,id)
           values(v_machnum,v_createuserid,trunc(sysdate),v_ip,v_iport,v_itype,v_machinetype,v_systemname,v_roomid,v_machinename,v_family,v_channo,v_id);
  else
    update room_machine_history set removeuserid = null, removedate = null where mach_num = v_machnum;
  end if;
  --添加新的机床信息
  insert into room_machine(mach_num,ip,iport,itype,machine_type,system_name,room_id,machine_name,family,channo,id)
         values(v_machnum,v_ip,v_iport,v_itype,v_machinetype,v_systemname,v_roomid,v_machinename,v_family,v_channo,v_id);

  --添加machine_picture表中的信息，这里仅添加mach_num
  insert into machine_picture(mach_num,picture_run,picture_stop,picture_alarm_offline_lack,picture_low,picture_high,aleft,top,awidth,height)
         values(v_machnum,v_picturerun,v_picturestop,v_picturealarmofflinelack,v_picturelow,v_picturehigh,v_aleft,v_top,v_awidth,v_height);
  -- 添加machine_process表中的信息，这里仅添加machnum
  insert into machine_process(mach_num, material, artificial) values (v_machnum, 0, 0);
  --添加machine_record操作信息
  insert into machine_record(mach_num,modifyuserid,modifydate,ip,iport,itype,machine_type,system_name,room_id,machine_name,family,channo,id)
         values(v_machnum,v_createuserid,sysdate,v_ip,v_iport,v_itype,v_machinetype,v_systemname,v_roomid,v_machinename,v_family,v_channo,v_id);
  --添加sensormach_configure表中的配置信息，这里仅添加machnum
  if (v_itype > 3000 and v_itype < 4000) then
    insert into sensormach_configure(mach_num,red,yellow,green,estop,spindlea,loadthreshold,expression)
           values(v_machnum,v_red,v_yellow,v_green,v_estop,v_spindlea,v_loadthreshold,v_expression);
  end if;
  insert into speed_alarm(mach_num,slarge,ssmall,flarge,fsmall) values(v_machnum,120,50,150,0);
  --添加worktime_flag表中的信息
  insert into worktime_flag(mach_num,flag) values(v_machnum,0);
  -- 添加pow_log_today表种的信息
  insert into pow_log_today(device_num) values(v_machnum);


  --添加workshop表中的信息
  -- insert into workshop(id,workshop_name,unit_name,section_name,mach_num) values(SEQ_WORKSHOP_ID.NEXTVAL,v_workshopname,v_unitname,v_sectionname,v_machnum);
  commit;
  v_msg := 1;

 /*
  *此部分为异常操作
  */
exception
  when my_machnumexist_exp then
    rollback;
    v_msg := 2;
  when my_useridnoexist_exp then
    rollback;
    v_msg := 3;
  when my_ipexist_exp then
    rollback;
    v_msg := 4;
  when others then
    rollback;
     if (v_flag_table < 9) then
      v_tablename := 'proglog_' || v_machnum;
      v_sql      := 'drop table ' || v_tablename;
      execute immediate v_sql;
    if (v_flag_table < 8) then
      v_tablename := 'pow_history_' || v_machnum;
      v_sql      := 'drop table ' || v_tablename;
      execute immediate v_sql;
      if (v_flag_table < 7) then
        v_tablename := 'error_' || v_machnum;
        v_sql       := 'drop table ' || v_tablename;
        execute immediate v_sql;
        if (v_flag_table < 6) then
        /*  v_sequence := 'seq_speedalarm_' || v_machnum || '_id';
          v_sql      := 'drop sequence ' || v_sequence;
          execute immediate v_sql;*/
          if (v_flag_table < 5) then
            v_tablename := 'speedalarm_' || v_machnum;
            v_sql       := 'drop table ' || v_tablename;
            execute immediate v_sql;
            if (v_flag_table < 4) then
              v_tablename := 'error_set_' || v_machnum;
              v_sql       := 'drop table ' || v_tablename;
              execute immediate v_sql;
              if (v_flag_table < 3) then
                v_tablename := 'run_set_' || v_machnum;
                v_sql       := 'drop table ' || v_tablename;
                execute immediate v_sql;
                if (v_flag_table < 2) then
                  v_tablename := 'status_' || v_machnum;
                  v_sql       := 'drop table ' || v_tablename;
                  execute immediate v_sql;
                  if (v_flag_table < 1) then
                    v_tablename := 'rstatus_' || v_machnum;
                    v_sql       := 'drop table ' || v_tablename;
                    execute immediate v_sql;
                  end if;
                end if;
              end if;
            end if;
          end if;
        end if;
      end if;
    end if;
    end if;
    v_msg := 5;
end pro_RoomMachineByMachNumInsert;
/

prompt
prompt Creating procedure PRO_ROOMMACHINEBYMACHNUMUPDATE
prompt =================================================
prompt
create or replace procedure lm04.PRO_ROOMMACHINEBYMACHNUMUPDATE(v_machnum in varchar2,v_createuserid in varchar2,v_ip in varchar2, v_iport in number,
                                                           v_itype in number,v_machinetype in varchar2,v_systemname in varchar2,v_roomid in varchar2,
                                                           v_machinename in varchar2,v_family in varchar2,v_channo in number,v_id in varchar2,v_picturerun in varchar2,
                                                           v_picturestop in varchar2,v_picturealarmofflinelack in varchar2,v_picturelow in varchar2,v_picturehigh in varchar2,
                                                           v_aleft in number,v_top in number,v_awidth in number,v_height in number,v_red in number,v_yellow in number,
                                                           v_green in number,v_estop in number,v_spindlea in number, v_loadthreshold in number,v_expression in varchar2,
                                                           v_unitname in varchar2,v_sectionname in varchar2,v_msg out number) is
my_2291_exp EXCEPTION;
PRAGMA EXCEPTION_INIT(my_2291_exp,-2291);--定义外键异常
v_flag number;--判断存在标志
begin
  update room_machine set ip=v_ip,iport=v_iport,itype=v_itype,machine_type=v_machinetype,system_name=v_systemname,room_id=v_roomid, machine_name= v_machinename,family=v_family,channo=v_channo,id=v_id where mach_num=v_machnum;
  update room_machine_history set ip=v_ip,iport=v_iport,itype=v_itype,machine_type=v_machinetype,system_name=v_systemname,room_id=v_roomid, machine_name= v_machinename,family=v_family,channo=v_channo,id=v_id where mach_num=v_machnum;
  update machine_picture set picture_run=v_picturerun,picture_stop=v_picturestop,picture_alarm_offline_lack=v_picturealarmofflinelack,picture_low=v_picturelow,picture_high=v_picturehigh,aleft=v_aleft,top=v_top,awidth=v_awidth,height=v_height where mach_num=v_machnum;
  insert into machine_record(mach_num,modifyuserid,modifydate,ip, iport, itype,machine_type,system_name,room_id, machine_name,family,channo,id) values
                          (v_machnum,v_createuserid,sysdate,v_ip,v_iport, v_itype,v_machinetype,v_systemname,v_roomid ,v_machinename,v_family,v_channo,v_id);
  update workshop set workshop_name=v_roomid,unit_name=v_unitname,section_name=v_sectionname where  mach_num=v_machnum;
  if(v_itype<3000 or v_itype>4000) then
  delete from sensormach_configure where mach_num=v_machnum;
  end if;
   if(v_itype>3000 and v_itype<4000) then
   select count(mach_num) into v_flag from sensormach_configure where mach_num=v_machnum;
   if v_flag=0 then
      insert into sensormach_configure(mach_num,red,yellow,green,estop,spindlea,loadthreshold,expression) values (v_machnum,v_red,v_yellow,v_green,v_estop,v_spindlea,v_loadthreshold,v_expression);
   end if;
   if v_flag>0 then
      update sensormach_configure set red=v_red,yellow=v_yellow,green=v_green,estop=v_estop,spindlea=v_spindlea,loadthreshold=v_loadthreshold,expression=v_expression where mach_num=v_machnum;
   end if;
   end if;
  commit;
  v_msg:=1;
  --异常信息
  exception
      when my_2291_exp then
         rollback;
         v_msg := 2;

         when others then
           rollback;
           v_msg:=3;
end PRO_ROOMMACHINEBYMACHNUMUPDATE;
/

prompt
prompt Creating procedure PRO_TIMEMODIFY
prompt =================================
prompt
create or replace procedure lm04.PRO_TIMEMODIFY is
v_second number;
--v_today_poweron_time number;
v_poweron_time number;
v_run_time number;
v_cut_time number;
begin
  select ceil(((select last_date from user_jobs where what='begin PRO_ZEROINSERT;end;')-(select trunc(sysdate) from dual))*24*60*60) into v_second from dual;
   declare
    cursor cur is
      select *  from machine_log_today where machine_online=1;
    begin
    for x in cur loop
      --select today_poweron_time into v_today_poweron_time from machine_log_today where mach_num=x.mach_num;
      --v_today_poweron_time:=v_today_poweron_time+v_second;
      --update machine_log_today set today_poweron_time=v_today_poweron_time where mach_num=x.mach_num ;
	  select poweron_time,run_time,cut_time into v_poweron_time, v_run_time,v_cut_time from worktime where x.mach_num=mach_num and insert_time=(select trunc(SYSDATE)-1/86400 from dual);
      v_poweron_time:=v_poweron_time-v_second;
      if v_poweron_time>86400 then
        v_poweron_time:=86400;
      end if;
      if v_poweron_time < 0 then
        v_poweron_time := 0;
      end if;
       if v_run_time>86400 then
        v_run_time:=86400;
      end if;
       if v_cut_time>86400 then
        v_cut_time:=86400;
      end if;
      update worktime set poweron_time=v_poweron_time,run_time=v_run_time,cut_time=v_cut_time where x.mach_num=mach_num and insert_time=(select trunc(SYSDATE)-1/86400 from dual);

      end loop;
     end;
    commit;
end PRO_TIMEMODIFY;
/

prompt
prompt Creating procedure PRO_ZEROINSERT
prompt =================================
prompt
create or replace procedure lm04.PRO_ZEROINSERT is
  v_TODAY_POWERON_TIME number;
  v_TODAY_RUN_TIME     number;
  v_TODAY_CUT_TIME     number;
  v_HOUR               number;
  v_MINUTE             number;
  v_SECOND             number;
  v_PEOPLE             varchar2(32);

begin
  select to_char(sysdate, 'hh24') into v_HOUR from dual;
  select to_char(sysdate, 'mi') into v_MINUTE from dual;
  select to_char(sysdate, 'ss') into v_SECOND from dual;
    declare
      cursor cur is
        select MACH_NUM from MACHINE_LOG_TODAY;
    begin
      for x in cur loop
        select TODAY_POWERON_TIME
          into v_TODAY_POWERON_TIME
          from MACHINE_LOG_TODAY
         where MACH_NUM = x.MACH_NUM;
        select TODAY_RUN_TIME
          into v_TODAY_RUN_TIME
          from MACHINE_LOG_TODAY
         where MACH_NUM = x.MACH_NUM;
        select TODAY_CUT_TIME
          into v_TODAY_CUT_TIME
          from MACHINE_LOG_TODAY
         where MACH_NUM = x.MACH_NUM;
        select ID
          into v_PEOPLE
          from ROOM_MACHINE
          where MACH_NUM = x.MACH_NUM;
          if v_TODAY_POWERON_TIME < 0 then
            v_TODAY_POWERON_TIME := 0;
          end if;
          if v_TODAY_RUN_TIME < 0 then
            v_TODAY_RUN_TIME := 0;
          end if;
          if v_TODAY_CUT_TIME < 0 then
            v_TODAY_CUT_TIME := 0;
          end if;

        insert into WORKTIME
          (MACH_NUM, USERID,POWERON_TIME, RUN_TIME, CUT_TIME, INSERT_TIME)
        values
          (x.MACH_NUM,
           v_PEOPLE,
           v_TODAY_POWERON_TIME,
           v_TODAY_RUN_TIME,
           v_TODAY_CUT_TIME,
           (select trunc(SYSDATE) - 1 / 86400 from dual));
        if v_HOUR = 0 and v_MINUTE = 0 and v_SECOND < 30 then --正常时间插入零点
        update WORKTIME_FLAG set flag = 1 WHERE MACH_NUM = x.MACH_NUM;
        else--非正常时间插入零点，如插入零点时间断电
          update MACHINE_LOG_TODAY set TODAY_POWERON_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update MACHINE_LOG_TODAY set TODAY_RUN_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update MACHINE_LOG_TODAY set TODAY_CUT_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update MACHINE_LOG_TODAY set DAY_POWERON_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update MACHINE_LOG_TODAY set DAY_RUN_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update MACHINE_LOG_TODAY set DAY_CUT_TIME = 0 WHERE MACH_NUM = x.MACH_NUM;
          update WORKTIME_FLAG set flag = 0 WHERE MACH_NUM = x.MACH_NUM;
        end if;
      end loop;
    end;
    commit;
exception
  when others then
    rollback;
end PRO_ZEROINSERT;
/

prompt
prompt Creating trigger TRG_ERROR_RBI
prompt ==============================
prompt
create or replace trigger LM04.TRG_ERROR_RBI
  before insert on error
  for each row
BEGIN
  SELECT SEQ_ERROR_LOG_ID.NEXTVAL INTO :NEW.LOG_ID FROM DUAL;
END TRG_ERROR_RBI;
/

prompt
prompt Creating trigger TRG_FIX_RECORD_RBI
prompt ===================================
prompt
create or replace trigger LM04.TRG_FIX_RECORD_RBI
  before insert on fix_record
  for each row
BEGIN
  SELECT SEQ_FIX_ID.NEXTVAL INTO :NEW.FIX_ID FROM DUAL;
END TRG_FIX_RECORD_RBI;
/

prompt
prompt Creating trigger TRG_FIX_RECORD_SYS_RBI
prompt =======================================
prompt
create or replace trigger LM04.TRG_FIX_RECORD_SYS_RBI
  before insert on fix_record_sys
  for each row
BEGIN
  SELECT SEQ_FIX_SYS_ID.NEXTVAL INTO :NEW.FIX_ID FROM DUAL;
END TRG_FIX_RECORD_SYS_RBI;
/

prompt
prompt Creating trigger TRG_LOOK_HISTORY_RBI
prompt =====================================
prompt
create or replace trigger LM04.TRG_LOOK_HISTORY_RBI
  before insert on look_history
  for each row
BEGIN
  SELECT SEQ_LOOK_HISTORY_ID.NEXTVAL INTO :NEW.ID FROM DUAL;
END TRG_LOOK_HISTORY_RBI;
/

prompt
prompt Creating trigger TRG_MACHINE_LOG_STATISTICS_RBI
prompt ===============================================
prompt
create or replace trigger LM04.TRG_MACHINE_LOG_STATISTICS_RBI
  before insert on machine_log_statistics
  for each row
BEGIN
  SELECT SEQ_STATISTICS_ID.NEXTVAL INTO :NEW.ID FROM DUAL;
END TRG_MACHINE_LOG_STATISTICS_RBI;
/

prompt
prompt Creating trigger TRG_MACHINE_RECORD_RBI
prompt =======================================
prompt
create or replace trigger LM04.TRG_MACHINE_RECORD_RBI
  before insert on machine_record
  for each row
BEGIN
  SELECT SEQ_MACHINE_RECORD_ID.NEXTVAL INTO :NEW.RECORDID FROM DUAL;
END TRG_MACHINE_RECORD_RBI;
/

prompt
prompt Creating trigger TRG_SPEEDALARM_RBI
prompt ===================================
prompt
create or replace trigger LM04.TRG_SPEEDALARM_RBI
  before insert on machine_speedalarm
  for each row
BEGIN
  SELECT SEQ_SPEEDALARM_ID.NEXTVAL INTO :NEW.ID FROM DUAL;
END TRG_SPEEDALARM_RBI;
/

prompt
prompt Creating trigger TRG_WORKSHOP_RBI
prompt =================================
prompt
create or replace trigger LM04.TRG_WORKSHOP_RBI
  before insert on workshop
  for each row
BEGIN
  SELECT SEQ_WORKSHOP_ID.NEXTVAL INTO :NEW.ID FROM DUAL;
END TRG_WORKSHOP_RBI;
/


spool off
