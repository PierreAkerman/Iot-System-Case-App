CREATE TABLE DeviceInfo(
DeviceId nvarchar(450) not null primary key,
DeviceType nvarchar(max) not null,
DeviceName nvarchar(max) null,
Location nvarchar(max) null,
Owner nvarchar(max) null,
ConnectionString nvarchar(max) null
)