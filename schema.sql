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
