DROP TABLE IF EXISTS "MonthlyTransactions";
DROP TABLE IF EXISTS "RecurringEntries";
DROP TABLE IF EXISTS "RecurringItems";
DROP TABLE IF EXISTS "AllocationSources";
DROP TABLE IF EXISTS "Allocations";
DROP TABLE IF EXISTS "Transactions";
DROP TABLE IF EXISTS "Accounts";
DROP TABLE IF EXISTS "MonthlyRecords";
DROP TABLE IF EXISTS "Members";
DROP TABLE IF EXISTS "Profiles";
DROP TABLE IF EXISTS "Users";

CREATE TABLE "Users" (
  "Id" uuid PRIMARY KEY,
  "DisplayName" text NOT NULL,
  "Email" varchar(255) NOT NULL,
  "PasswordHash" text NOT NULL,
  "IsSuperAdmin" boolean NOT NULL DEFAULT FALSE,
  "CreatedAt" timestamptz NOT NULL
);
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE TABLE "Profiles" (
  "Id" uuid PRIMARY KEY,
  "Name" text NOT NULL,
  "Currency" char(3) NOT NULL DEFAULT 'EUR',
  "CreatedAt" timestamptz NOT NULL
);

CREATE TABLE "Members" (
  "Id" uuid PRIMARY KEY,
  "UserId" uuid NOT NULL,
  "ProfileId" uuid NOT NULL,
  "DisplayName" text NOT NULL,
  "Email" text NOT NULL,
  "Role" text NOT NULL,
  "JoinedAt" timestamptz NOT NULL
);
CREATE UNIQUE INDEX "IX_Members_ProfileId_UserId" ON "Members" ("ProfileId", "UserId");
CREATE UNIQUE INDEX "IX_Members_ProfileId_Email" ON "Members" ("ProfileId", "Email");

CREATE TABLE "Accounts" (
  "Id" uuid PRIMARY KEY,
  "ProfileId" uuid NOT NULL,
  "Name" text NOT NULL,
  "Type" text NOT NULL,
  "Institution" text NULL,
  "BalanceOverride" numeric(18,2) NULL,
  "Currency" char(3) NOT NULL DEFAULT 'EUR',
  "SyncedAt" timestamptz NULL,
  "IsActive" boolean NOT NULL DEFAULT TRUE,
  "CreatedAt" timestamptz NOT NULL
);

CREATE TABLE "Transactions" (
  "Id" uuid PRIMARY KEY,
  "AccountId" uuid NOT NULL,
  "LoggedBy" uuid NULL,
  "Amount" numeric(18,2) NOT NULL,
  "Direction" text NOT NULL,
  "Description" varchar(200) NOT NULL,
  "Note" varchar(1000) NULL,
  "Date" date NOT NULL,
  "Category" text NOT NULL,
  "CreatedAt" timestamptz NOT NULL,
  CONSTRAINT "FK_Transactions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_Transactions_AccountId" ON "Transactions" ("AccountId");
CREATE INDEX "IX_Transactions_Date" ON "Transactions" ("Date");

CREATE TABLE "Allocations" (
  "Id" uuid PRIMARY KEY,
  "ProfileId" uuid NOT NULL,
  "Name" text NOT NULL,
  "Purpose" text NULL,
  "TargetAmount" numeric(18,2) NOT NULL,
  "CurrentAmount" numeric(18,2) NOT NULL,
  "Status" text NOT NULL,
  "TargetDate" date NULL,
  "CreatedAt" timestamptz NOT NULL
);

CREATE TABLE "AllocationSources" (
  "Id" uuid PRIMARY KEY,
  "AllocationId" uuid NOT NULL,
  "AccountId" uuid NOT NULL,
  "Amount" numeric(18,2) NOT NULL
);

CREATE TABLE "RecurringItems" (
  "Id" uuid PRIMARY KEY,
  "ProfileId" uuid NOT NULL,
  "AccountId" uuid NULL,
  "Name" text NOT NULL,
  "Category" text NOT NULL,
  "ExpectedAmount" numeric(18,2) NOT NULL,
  "Frequency" text NOT NULL,
  "DayOfMonth" smallint NOT NULL,
  "AutoLog" boolean NOT NULL DEFAULT FALSE,
  "IsActive" boolean NOT NULL DEFAULT TRUE,
  "CreatedAt" timestamptz NOT NULL
);

CREATE TABLE "MonthlyRecords" (
  "Id" uuid PRIMARY KEY,
  "ProfileId" uuid NOT NULL,
  "Month" smallint NOT NULL,
  "Year" smallint NOT NULL,
  "IncomeTotal" numeric(18,2) NOT NULL,
  "SpendingTotal" numeric(18,2) NOT NULL,
  "RecurringTotal" numeric(18,2) NOT NULL,
  "AllocationTotal" numeric(18,2) NOT NULL,
  "Leftover" numeric(18,2) NOT NULL,
  "Status" text NOT NULL,
  "OpenedAt" timestamptz NOT NULL,
  "ClosedAt" timestamptz NULL
);

CREATE TABLE "MonthlyTransactions" (
  "Id" uuid PRIMARY KEY,
  "MonthlyRecordId" uuid NOT NULL,
  "TransactionId" uuid NOT NULL
);

CREATE TABLE "RecurringEntries" (
  "Id" uuid PRIMARY KEY,
  "MonthlyRecordId" uuid NOT NULL,
  "RecurringItemId" uuid NOT NULL,
  "ExpectedAmount" numeric(18,2) NOT NULL,
  "ActualAmount" numeric(18,2) NULL,
  "Confirmed" boolean NOT NULL DEFAULT FALSE,
  "PaidDate" date NULL,
  "TransactionId" uuid NULL
);
