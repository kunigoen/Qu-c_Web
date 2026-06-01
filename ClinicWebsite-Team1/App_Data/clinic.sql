CREATE DATABASE ClinicWebsite
GO
USE ClinicWebsite
GO
CREATE DATABASE ClinicWebsite;
GO
USE ClinicWebsite;
GO

CREATE TABLE user_account(
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) NOT NULL UNIQUE,
    password_hash NVARCHAR(500) NOT NULL,
    full_name NVARCHAR(150) NOT NULL,
    phone_number VARCHAR(20),
    avatar_url NVARCHAR(500),
    role VARCHAR(20) NOT NULL CHECK(role IN ('Admin','Doctor','Patient')),
    is_active BIT DEFAULT 1,
    failed_login_count INT DEFAULT 0,
    lockout_end DATETIME NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME NULL
);
select * from user_account

CREATE TABLE specialty(
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(1000),
    image_url NVARCHAR(500)
);

CREATE TABLE patient(
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_account_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES user_account(id),
    date_of_birth DATE,
    gender VARCHAR(10) CHECK(gender IN ('Male','Female','Other')),
    address NVARCHAR(500),
    emergency_contact NVARCHAR(150),
    blood_type VARCHAR(5),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME NULL
);

CREATE TABLE doctor(
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_account_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES user_account(id),
    specialty_id INT NOT NULL FOREIGN KEY REFERENCES specialty(id),
    license_number VARCHAR(100) UNIQUE,
    experience_years INT DEFAULT 0,
    consultation_fee DECIMAL(18,2) NOT NULL,
    biography NVARCHAR(MAX),
    image_url NVARCHAR(500),
    status VARCHAR(20) CHECK(status IN ('Active','Inactive','OnLeave')),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME NULL
);

CREATE TABLE clinic_room(
    id INT IDENTITY(1,1) PRIMARY KEY,
    room_code VARCHAR(20) NOT NULL UNIQUE,
    room_name NVARCHAR(100),
    floor_number INT,
    status VARCHAR(20) CHECK(status IN ('Available','Maintenance'))
);

CREATE TABLE schedule(
    id INT IDENTITY(1,1) PRIMARY KEY,
    doctor_id INT NOT NULL FOREIGN KEY REFERENCES doctor(id),
    room_id INT NOT NULL FOREIGN KEY REFERENCES clinic_room(id),
    work_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    max_patients INT NOT NULL,
    booked_patients INT DEFAULT 0,
    status VARCHAR(20) CHECK(status IN ('Available','Full','Closed')),
    created_at DATETIME DEFAULT GETDATE()
);

CREATE TABLE appointment_status(
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE appointment(
    id INT IDENTITY(1,1) PRIMARY KEY,
    patient_id INT NOT NULL FOREIGN KEY REFERENCES patient(id),
    doctor_id INT NOT NULL FOREIGN KEY REFERENCES doctor(id),
    schedule_id INT NOT NULL FOREIGN KEY REFERENCES schedule(id),
    status_id INT NOT NULL FOREIGN KEY REFERENCES appointment_status(id),
    symptoms NVARCHAR(1000),
    note NVARCHAR(1000),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME NULL
);

CREATE TABLE medical_service(
    id INT IDENTITY(1,1) PRIMARY KEY,
    service_name NVARCHAR(150) NOT NULL,
    description NVARCHAR(1000),
    price DECIMAL(18,2) NOT NULL,
    duration_minutes INT,
    status BIT DEFAULT 1
);

CREATE TABLE appointment_service(
    appointment_id INT NOT NULL FOREIGN KEY REFERENCES appointment(id),
    service_id INT NOT NULL FOREIGN KEY REFERENCES medical_service(id),
    quantity INT DEFAULT 1,
    unit_price DECIMAL(18,2) NOT NULL,
    PRIMARY KEY(appointment_id,service_id)
);

CREATE TABLE consultation(
    id INT IDENTITY(1,1) PRIMARY KEY,
    appointment_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES appointment(id),
    diagnosis NVARCHAR(MAX),
    prescription NVARCHAR(MAX),
    doctor_notes NVARCHAR(MAX),
    consultation_date DATETIME DEFAULT GETDATE()
);

CREATE TABLE payment(
    id INT IDENTITY(1,1) PRIMARY KEY,
    appointment_id INT NOT NULL FOREIGN KEY REFERENCES appointment(id),
    amount DECIMAL(18,2) NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    transaction_code VARCHAR(200),
    payment_date DATETIME,
    status VARCHAR(20)
);

CREATE TABLE doctor_review(
    id INT IDENTITY(1,1) PRIMARY KEY,
    appointment_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES appointment(id),
    doctor_id INT NOT NULL FOREIGN KEY REFERENCES doctor(id),
    patient_id INT NOT NULL FOREIGN KEY REFERENCES patient(id),
    rating INT CHECK(rating BETWEEN 1 AND 5),
    comment NVARCHAR(1000),
    review_date DATETIME DEFAULT GETDATE()
);

INSERT INTO specialty(name,description,image_url) VALUES
(N'Tim mạch',N'Khám tim mạch','timmach.jpg'),
(N'Da liễu',N'Khám da liễu','dalieu.jpg'),
(N'Nhi khoa',N'Khám nhi khoa','nhikhoa.jpg'),
(N'Thần kinh',N'Khám thần kinh','thankinh.jpg'),
(N'Xương khớp',N'Khám xương khớp','xuongkhop.jpg'),
(N'Tai Mũi Họng',N'Khám TMH','tmh.jpg'),
(N'Nhãn khoa',N'Khám mắt','mat.jpg'),
(N'Nội tổng quát',N'Nội tổng quát','noi.jpg'),
(N'Tiêu hóa',N'Tiêu hóa','tieuhoa.jpg'),
(N'Sản phụ khoa',N'Sản phụ khoa','sanphu.jpg');

INSERT INTO clinic_room(room_code,room_name,floor_number,status) VALUES
('A101',N'Phòng 1',1,'Available'),
('A102',N'Phòng 2',1,'Available'),
('B201',N'Phòng 3',2,'Available'),
('B202',N'Phòng 4',2,'Available'),
('C301',N'Phòng 5',3,'Available');

INSERT INTO appointment_status(name) VALUES
('Pending'),('Confirmed'),('Completed'),('Cancelled');

-- 15 doctors
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor1@clinic.com','HASH123',N'Nguyễn Văn Minh','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor2@clinic.com','HASH123',N'Trần Quốc Bảo','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor3@clinic.com','HASH123',N'Lê Hoàng Nam','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor4@clinic.com','HASH123',N'Phạm Đức Anh','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor5@clinic.com','HASH123',N'Võ Thanh Tùng','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor6@clinic.com','HASH123',N'Đặng Minh Quân','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor7@clinic.com','HASH123',N'Ngô Hải Long','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor8@clinic.com','HASH123',N'Bùi Gia Huy','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor9@clinic.com','HASH123',N'Phan Tuấn Kiệt','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor10@clinic.com','HASH123',N'Huỳnh Quốc Thái','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor11@clinic.com','HASH123',N'Đỗ Thành Công','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor12@clinic.com','HASH123',N'Trịnh Minh Khoa','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor13@clinic.com','HASH123',N'John Smith','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor14@clinic.com','HASH123',N'David Wilson','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('doctor15@clinic.com','HASH123',N'Emily Johnson','Doctor');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient1@clinic.com','HASH123',N'Nguyễn Văn An','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient2@clinic.com','HASH123',N'Trần Minh Khang','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient3@clinic.com','HASH123',N'Lê Thu Trang','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient4@clinic.com','HASH123',N'Phạm Gia Hân','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient5@clinic.com','HASH123',N'Võ Hoàng Yến','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient6@clinic.com','HASH123',N'Đặng Khánh Linh','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient7@clinic.com','HASH123',N'Ngô Đức Huy','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient8@clinic.com','HASH123',N'Bùi Nhật Nam','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient9@clinic.com','HASH123',N'Phan Mỹ Tiên','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient10@clinic.com','HASH123',N'Huỳnh Tuấn Anh','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient11@clinic.com','HASH123',N'Đỗ Ngọc Mai','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient12@clinic.com','HASH123',N'Trịnh Gia Bảo','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient13@clinic.com','HASH123',N'Michael Brown','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient14@clinic.com','HASH123',N'Sophia Davis','Patient');
INSERT INTO user_account(email,password_hash,full_name,role) VALUES ('patient15@clinic.com','HASH123',N'Emma Taylor','Patient');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (1,1,'LIC001',6,250000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (2,2,'LIC002',7,300000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (3,3,'LIC003',8,350000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (4,4,'LIC004',9,400000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (5,5,'LIC005',10,450000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (6,6,'LIC006',11,500000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (7,7,'LIC007',12,550000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (8,8,'LIC008',13,600000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (9,9,'LIC009',14,650000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (10,10,'LIC010',5,700000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (11,1,'LIC011',6,750000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (12,2,'LIC012',7,800000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (13,3,'LIC013',8,850000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (14,4,'LIC014',9,900000,'Active');
INSERT INTO doctor(user_account_id,specialty_id,license_number,experience_years,consultation_fee,status) VALUES (15,5,'LIC015',10,950000,'Active');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (16,'1991-01-02','Male',N'TP.HCM',N'0900000001','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (17,'1992-01-03','Female',N'TP.HCM',N'0900000002','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (18,'1993-01-04','Male',N'TP.HCM',N'0900000003','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (19,'1994-01-05','Female',N'TP.HCM',N'0900000004','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (20,'1995-01-06','Male',N'TP.HCM',N'0900000005','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (21,'1996-01-07','Female',N'TP.HCM',N'0900000006','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (22,'1997-01-08','Male',N'TP.HCM',N'0900000007','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (23,'1998-01-09','Female',N'TP.HCM',N'0900000008','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (24,'1999-01-10','Male',N'TP.HCM',N'0900000009','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (25,'1990-01-11','Female',N'TP.HCM',N'0900000010','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (26,'1991-01-12','Male',N'TP.HCM',N'0900000011','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (27,'1992-01-13','Female',N'TP.HCM',N'0900000012','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (28,'1993-01-14','Male',N'TP.HCM',N'0900000013','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (29,'1994-01-15','Female',N'TP.HCM',N'0900000014','O+');
INSERT INTO patient(user_account_id,date_of_birth,gender,address,emergency_contact,blood_type) VALUES (30,'1995-01-16','Male',N'TP.HCM',N'0900000015','O+');

INSERT INTO medical_service(service_name,description,price,duration_minutes) VALUES
(N'Khám tổng quát',N'',200000,15),
(N'Khám tim mạch',N'',350000,20),
(N'Khám da liễu',N'',250000,15),
(N'Siêu âm',N'',300000,20),
(N'Xét nghiệm máu',N'',180000,10),
(N'Đo điện tim',N'',220000,15),
(N'Khám mắt',N'',250000,15),
(N'Khám nhi',N'',200000,15),
(N'Nội soi',N'',800000,30),
(N'Tư vấn chuyên sâu',N'',500000,30);
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (1,1,'2026-06-02','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (2,2,'2026-06-03','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (3,3,'2026-06-04','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (4,4,'2026-06-05','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (5,5,'2026-06-06','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (6,1,'2026-06-07','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (7,2,'2026-06-08','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (8,3,'2026-06-09','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (9,4,'2026-06-10','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (10,5,'2026-06-11','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (11,1,'2026-06-12','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (12,2,'2026-06-13','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (13,3,'2026-06-14','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (14,4,'2026-06-15','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (15,5,'2026-06-16','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (1,1,'2026-06-17','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (2,2,'2026-06-18','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (3,3,'2026-06-19','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (4,4,'2026-06-20','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (5,5,'2026-06-21','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (6,1,'2026-06-22','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (7,2,'2026-06-23','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (8,3,'2026-06-24','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (9,4,'2026-06-25','08:00','09:00',5,0,'Available');
INSERT INTO schedule(doctor_id,room_id,work_date,start_time,end_time,max_patients,booked_patients,status) VALUES (10,5,'2026-06-26','08:00','09:00',5,0,'Available');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (1,1,1,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (2,2,2,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (3,3,3,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (4,4,4,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (5,5,5,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (6,6,6,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (7,7,7,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (8,8,8,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (9,9,9,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (10,10,10,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (11,11,11,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (12,12,12,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (13,13,13,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (14,14,14,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (15,15,15,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (1,1,16,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (2,2,17,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (3,3,18,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (4,4,19,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (5,5,20,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (6,6,21,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (7,7,22,2,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (8,8,23,3,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (9,9,24,4,N'Đau đầu',N'Đặt lịch online');
INSERT INTO appointment(patient_id,doctor_id,schedule_id,status_id,symptoms,note) VALUES (10,10,25,1,N'Đau đầu',N'Đặt lịch online');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (1,300000,'VNPay','TXN0001',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (2,300000,'VNPay','TXN0002',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (3,300000,'VNPay','TXN0003',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (4,300000,'VNPay','TXN0004',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (5,300000,'VNPay','TXN0005',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (6,300000,'VNPay','TXN0006',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (7,300000,'VNPay','TXN0007',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (8,300000,'VNPay','TXN0008',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (9,300000,'VNPay','TXN0009',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (10,300000,'VNPay','TXN0010',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (11,300000,'VNPay','TXN0011',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (12,300000,'VNPay','TXN0012',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (13,300000,'VNPay','TXN0013',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (14,300000,'VNPay','TXN0014',GETDATE(),'Paid');
INSERT INTO payment(appointment_id,amount,payment_method,transaction_code,payment_date,status) VALUES (15,300000,'VNPay','TXN0015',GETDATE(),'Paid');


WITH cte AS (
    SELECT 
        id,
        ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM doctor
)
UPDATE d
SET image_url = '/Content/images/doctor' 
                + CAST(cte.rn AS NVARCHAR(10)) 
                + '.jpg'
FROM doctor d
JOIN cte ON d.id = cte.id;


WITH cte AS (
    SELECT 
        u.id,
        ROW_NUMBER() OVER (ORDER BY u.id) AS rn
    FROM user_account u
    WHERE u.role = 'Doctor'
)
UPDATE u
SET avatar_url = '/Content/images/doctor' 
                + CAST(cte.rn AS NVARCHAR(10)) 
                + '.jpg'
FROM user_account u
JOIN cte ON u.id = cte.id
WHERE u.role = 'Doctor';


WITH cte AS (
    SELECT 
        id,
        ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM user_account
)
UPDATE user_account
SET phone_number = '090' + RIGHT('0000000' + CAST(cte.rn AS VARCHAR(10)), 7)
FROM user_account u
JOIN cte ON u.id = cte.id;



-- PATIENT phone_number lấy từ user_account
UPDATE p
SET p.phone_number = u.phone_number
FROM patient p
JOIN user_account u ON p.user_account_id = u.id;
-- DOCTOR phone_number lấy từ user_account
UPDATE d
SET d.phone_number = u.phone_number
FROM doctor d
JOIN user_account u ON d.user_account_id = u.id;

ALTER TABLE patient
ADD phone_number VARCHAR(20);
ALTER TABLE doctor
ADD phone_number VARCHAR(20);
select * from doctor

SELECT COUNT(*) AS total_doctors
FROM doctor;

SELECT COUNT(*) AS total_patients
FROM patient;

SELECT COUNT(*) AS total_specialties
FROM specialty;

SELECT COUNT(*) AS total_schedules
FROM schedule;

SELECT COUNT(*) AS total_appointments
FROM appointment;

SELECT COUNT(*) AS total_payments
FROM payment;