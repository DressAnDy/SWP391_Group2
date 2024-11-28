CREATE DATABASE KoiBetv1;
USE KoiBetv1;

CREATE TABLE Roles (
	role_id VARCHAR(50) PRIMARY KEY NOT NULL,
	role_name VARCHAR(50) NOT NULL
);

CREATE TABLE Users (
	users_id VARCHAR(50) PRIMARY KEY NOT NULL,
	username VARCHAR(100) NOT NULL,
	full_name VARCHAR(100) NOT NULL,
	password VARCHAR(100) NOT NULL,
	email VARCHAR(100) NOT NULL,
	phone VARCHAR(100) NOT NULL,
	role_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (role_id) REFERENCES Roles(role_id),
	balance DECIMAL(10, 2)
);

ALTER TABLE Users
ALTER COLUMN full_name NVARCHAR(255) NULL;



CREATE TABLE Transactions (
	transactions_id VARCHAR(50) PRIMARY KEY NOT NULL,
	users_id VARCHAR(50) NOT NULL,
	amount DECIMAL(10, 2) NOT NULL,
	messages VARCHAR(255) NOT NULL,
	transactions_time DATE NOT NULL,
	FOREIGN KEY (users_id) REFERENCES Users(users_id)
);

CREATE TABLE Referee (
	referee_id VARCHAR(50) PRIMARY KEY NOT NULL,
	referee_name VARCHAR(50) NOT NULL,
	exp_judge VARCHAR(50) NOT NULL,
	users_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (users_id) REFERENCES Users(users_id)
);

CREATE TABLE FishKoi (
	koi_id VARCHAR(50) PRIMARY KEY NOT NULL,
	koi_name VARCHAR(100) NOT NULL,
	koi_variety VARCHAR(100) NOT NULL,
	koi_size VARCHAR(50) NOT NULL,
	koi_age VARCHAR(50) NOT NULL,
	users_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (users_id) REFERENCES Users(users_id)
);

CREATE TABLE KoiStandard (
	standard_id VARCHAR(50) PRIMARY KEY NOT NULL,
	color_koi VARCHAR(255) NOT NULL,
	pattern_koi VARCHAR(255) NOT NULL, 
	size_koi DECIMAL(10, 2) NOT NULL, 
	age_koi INT NOT NULL,
	bodyshape_koi VARCHAR(255) NOT NULL,
	variety_koi VARCHAR(100) NOT NULL,
	standard_name VARCHAR(255) NOT NULL,
	gender TINYINT NOT NULL
);

CREATE TABLE KoiCategory (
	category_id VARCHAR(50) PRIMARY KEY NOT NULL,
	category_name VARCHAR(50) NOT NULL,
	standard_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (standard_id) REFERENCES KoiStandard(standard_id)
);

CREATE TABLE Award (
	award_id VARCHAR(50) PRIMARY KEY NOT NULL,
	award_name VARCHAR(50) NOT NULL,
	quantity INT
);

CREATE TABLE CompetitionKoi (
	competition_id VARCHAR(50) PRIMARY KEY NOT NULL,
	competition_name VARCHAR(100) NOT NULL,
	competition_description VARCHAR(255),
	start_time DATE,
	end_time DATE,
	status_competition BIT NOT NULL,
	category_id VARCHAR(50) NOT NULL,
	koi_id VARCHAR(50) NOT NULL,
	referee_id VARCHAR(50) NOT NULL,
	award_id VARCHAR(50) NOT NULL,
	rounds VARCHAR(50) NOT NULL,
	FOREIGN KEY (category_id) REFERENCES KoiCategory(category_id),
	FOREIGN KEY (koi_id) REFERENCES FishKoi(koi_id),
	FOREIGN KEY (referee_id) REFERENCES Referee(referee_id),
	FOREIGN KEY (award_id) REFERENCES Award(award_id)
);

CREATE TABLE KoiRegistration (
	registration_id VARCHAR(50) PRIMARY KEY NOT NULL,
	koi_id VARCHAR(50) NOT NULL,
	competition_id VARCHAR(50) NOT NULL,
	status_registration BIT NOT NULL,
	category_id VARCHAR(50) NOT NULL,
	slot_registration INT NOT NULL,
	start_dates DATE,
	end_dates DATE,
	registration_fee DECIMAL(10, 2) NOT NULL,
	FOREIGN KEY (koi_id) REFERENCES FishKoi(koi_id),
	FOREIGN KEY (competition_id) REFERENCES CompetitionKoi(competition_id),
	FOREIGN KEY (category_id) REFERENCES KoiCategory(category_id)
);

CREATE TABLE CompetitionRound (
	round_id VARCHAR(50) PRIMARY KEY NOT NULL,
	match INT,
	competition_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (competition_id) REFERENCES CompetitionKoi(competition_id)
);

CREATE TABLE CompetitionMatch (
	match_id VARCHAR(50) PRIMARY KEY NOT NULL,
	first_koi NVARCHAR(100) NOT NULL,
	round_id VARCHAR(50) NOT NULL,
	second_koi NVARCHAR(100) NOT NULL,
	result NVARCHAR(100) NOT NULL,
	FOREIGN KEY (round_id) REFERENCES CompetitionRound(round_id)
);

CREATE TABLE KoiScore (
	score_id VARCHAR(50) PRIMARY KEY NOT NULL,
	koi_id VARCHAR(50) NOT NULL,
	referee_id VARCHAR(50) NOT NULL,
	match_id VARCHAR(50) NOT NULL,
	score_koi DECIMAL(10, 2) NOT NULL,
	FOREIGN KEY (koi_id) REFERENCES FishKoi(koi_id),
	FOREIGN KEY (referee_id) REFERENCES Referee(referee_id),
	FOREIGN KEY (match_id) REFERENCES CompetitionMatch(match_id)
);

CREATE TABLE KoiBet (
	bet_id VARCHAR(50) PRIMARY KEY NOT NULL,
	users_id VARCHAR(50) NOT NULL,
	registration_id VARCHAR(50) NOT NULL,
	competition_id VARCHAR(50) NOT NULL,
	FOREIGN KEY (users_id) REFERENCES Users(users_id),
	FOREIGN KEY (registration_id) REFERENCES KoiRegistration(registration_id),
	FOREIGN KEY (competition_id) REFERENCES CompetitionKoi(competition_id)
);
