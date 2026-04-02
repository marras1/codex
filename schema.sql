CREATE TYPE member_role AS ENUM ('owner', 'editor', 'viewer');
CREATE TYPE account_type AS ENUM ('cash','savings','stocks','etf','retirement','crypto','property','other');
CREATE TYPE transaction_direction AS ENUM ('debit','credit');
CREATE TYPE transaction_category AS ENUM (
  'groceries','dining','transport','fuel','utilities','rent_mortgage','loan_repayment',
  'insurance','healthcare','education','entertainment','clothing','electronics',
  'subscriptions','travel','gifts','income','transfer','investment','other');
CREATE TYPE allocation_status AS ENUM ('active','paused','completed');
CREATE TYPE recurring_frequency AS ENUM ('monthly','quarterly','annual');
CREATE TYPE monthly_record_status AS ENUM ('open','closing','locked');

-- Minimal multi-profile auth model additions
CREATE TABLE IF NOT EXISTS "Users" (
  id UUID PRIMARY KEY,
  display_name TEXT NOT NULL,
  email TEXT NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  is_super_admin BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "Profiles" (
  id UUID PRIMARY KEY,
  name TEXT NOT NULL,
  currency CHAR(3) NOT NULL DEFAULT 'EUR',
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "Members" (
  id UUID PRIMARY KEY,
  user_id UUID NOT NULL,
  profile_id UUID NOT NULL,
  display_name TEXT NOT NULL,
  email TEXT NOT NULL,
  role member_role NOT NULL DEFAULT 'editor',
  joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(profile_id, user_id)
);
