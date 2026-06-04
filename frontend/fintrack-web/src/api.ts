export type AccountType = 'BankAccount' | 'CreditCard' | 'Cash' | 'Investment'
export type CategoryType = 'Income' | 'Expense' | 'Both'
export type TransactionType = 'Income' | 'Expense'
export type RecurringFrequency = 'Monthly' | 'Weekly' | 'Yearly'
export type FileImportStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed'
export type FileImportType = 'Csv' | 'Excel' | 'Pdf'

export type Account = {
  id: string
  name: string
  type: AccountType
  initialBalance: number
  createdAt: string
}

export type Category = {
  id: string
  name: string
  type: CategoryType
  parentCategoryId: string | null
  createdAt: string
}

export type Transaction = {
  id: string
  accountId: string
  categoryId: string
  description: string
  amount: number
  type: TransactionType
  date: string
  dueDate: string | null
  isPaid: boolean
  paymentDate: string | null
  installmentGroupId: string | null
  recurringRuleId: string | null
  createdAt: string
}

export type CategorySummary = {
  categoryId: string
  categoryName: string
  total: number
}

export type MonthlySummary = {
  year: number
  month: number
  totalIncome: number
  totalExpense: number
  balance: number
  expensesByCategory: CategorySummary[]
  incomeByCategory: CategorySummary[]
  upcomingPayments: number
  paidExpenses: number
  unpaidExpenses: number
}

export type CreateTransactionRequest = {
  accountId: string
  categoryId: string
  description: string
  amount: number
  type: TransactionType
  date: string
  dueDate: string | null
  isPaid: boolean
  paymentDate: string | null
}

export type UpdateTransactionRequest = CreateTransactionRequest

export type CreateAccountRequest = {
  name: string
  type: AccountType
  initialBalance: number
}

export type UpdateAccountRequest = CreateAccountRequest

export type CreateCategoryRequest = {
  name: string
  type: CategoryType
  parentCategoryId: string | null
}

export type UpdateCategoryRequest = CreateCategoryRequest

export type CreateInstallmentPurchaseRequest = {
  accountId: string
  categoryId: string
  description: string
  totalAmount: number
  totalInstallments: number
  startDate: string
  dueDay: number | null
}

export type InstallmentGroup = {
  id: string
  description: string
  totalAmount: number
  installmentAmount: number
  totalInstallments: number
  startDate: string
  createdAt: string
}

export type CreateRecurringRuleRequest = {
  accountId: string
  categoryId: string
  description: string
  amount: number
  frequency: RecurringFrequency
  dayOfMonth: number
  startDate: string
  endDate: string | null
}

export type RecurringRule = {
  id: string
  accountId: string
  categoryId: string
  description: string
  amount: number
  frequency: RecurringFrequency
  dayOfMonth: number
  startDate: string
  endDate: string | null
  isActive: boolean
}

export type GenerateRecurringTransactionsResult = {
  createdTransactions: number
}

export type ForecastMonth = {
  year: number
  month: number
  income: number
  expense: number
  balance: number
  projectedRecurringExpenses: number
}

export type CsvImportRowPreview = {
  rowNumber: number
  description: string
  amount: number | null
  type: TransactionType | null
  date: string | null
  accountId: string | null
  categoryId: string | null
  dueDate: string | null
  isPaid: boolean | null
  paymentDate: string | null
  errors: string[]
}

export type CsvImportPreview = {
  totalRows: number
  validRows: number
  invalidRows: number
  rows: CsvImportRowPreview[]
}

export type ImportBatch = {
  id: string
  fileName: string
  fileType: FileImportType
  importedAt: string
  totalRows: number
  successRows: number
  failedRows: number
  status: FileImportStatus
}

export type CategoryKeywordRule = {
  id: string
  categoryId: string
  categoryName: string
  keyword: string
  transactionType: TransactionType | null
  priority: number
  isActive: boolean
  createdAt: string
}

export type CreateCategoryKeywordRuleRequest = {
  categoryId: string
  keyword: string
  transactionType: TransactionType | null
  priority: number
  isActive: boolean
}

export type UpdateCategoryKeywordRuleRequest = CreateCategoryKeywordRuleRequest

type TransactionFilters = {
  startDate?: string
  endDate?: string
  categoryId?: string
  accountId?: string
  type?: TransactionType | ''
  isPaid?: boolean | ''
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7000'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
    ...init,
  })

  if (!response.ok) {
    const message = await response
      .json()
      .then((body) => body.message as string | undefined)
      .catch(() => undefined)

    throw new Error(message ?? `Erro HTTP ${response.status}`)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

function toQueryString(params: Record<string, string | number | boolean | undefined>) {
  const query = new URLSearchParams()

  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== '') {
      query.set(key, String(value))
    }
  })

  const value = query.toString()
  return value ? `?${value}` : ''
}

export const api = {
  getAccounts: () => request<Account[]>('/api/accounts'),
  getCategories: () => request<Category[]>('/api/categories'),
  createAccount: (account: CreateAccountRequest) =>
    request<Account>('/api/accounts', {
      method: 'POST',
      body: JSON.stringify(account),
    }),
  updateAccount: (id: string, account: UpdateAccountRequest) =>
    request<void>(`/api/accounts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(account),
    }),
  deleteAccount: (id: string) =>
    request<void>(`/api/accounts/${id}`, {
      method: 'DELETE',
    }),
  createCategory: (category: CreateCategoryRequest) =>
    request<Category>('/api/categories', {
      method: 'POST',
      body: JSON.stringify(category),
    }),
  updateCategory: (id: string, category: UpdateCategoryRequest) =>
    request<void>(`/api/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(category),
    }),
  deleteCategory: (id: string) =>
    request<void>(`/api/categories/${id}`, {
      method: 'DELETE',
    }),
  getMonthlySummary: (year: number, month: number) =>
    request<MonthlySummary>(`/api/dashboard/monthly-summary${toQueryString({ year, month })}`),
  getTransactions: (filters: TransactionFilters = {}) =>
    request<Transaction[]>(`/api/transactions${toQueryString(filters)}`),
  createTransaction: (transaction: CreateTransactionRequest) =>
    request<Transaction>('/api/transactions', {
      method: 'POST',
      body: JSON.stringify(transaction),
    }),
  updateTransaction: (id: string, transaction: UpdateTransactionRequest) =>
    request<void>(`/api/transactions/${id}`, {
      method: 'PUT',
      body: JSON.stringify(transaction),
    }),
  deleteTransaction: (id: string) =>
    request<void>(`/api/transactions/${id}`, {
      method: 'DELETE',
    }),
  createInstallmentPurchase: (purchase: CreateInstallmentPurchaseRequest) =>
    request<InstallmentGroup>('/api/installments', {
      method: 'POST',
      body: JSON.stringify(purchase),
    }),
  getRecurringRules: () => request<RecurringRule[]>('/api/recurring-rules'),
  createRecurringRule: (rule: CreateRecurringRuleRequest) =>
    request<RecurringRule>('/api/recurring-rules', {
      method: 'POST',
      body: JSON.stringify(rule),
    }),
  generateRecurringTransactions: (throughDate: string) =>
    request<GenerateRecurringTransactionsResult>('/api/recurring-rules/generate', {
      method: 'POST',
      body: JSON.stringify({ throughDate }),
    }),
  getForecast: (year: number, month: number, months = 6) =>
    request<ForecastMonth[]>(`/api/forecast${toQueryString({ year, month, months })}`),
  previewCsvImport: (fileName: string, content: string) =>
    request<CsvImportPreview>('/api/imports/csv/preview', {
      method: 'POST',
      body: JSON.stringify({ fileName, content }),
    }),
  commitCsvImport: (fileName: string, content: string) =>
    request<ImportBatch>('/api/imports/csv/commit', {
      method: 'POST',
      body: JSON.stringify({ fileName, content }),
    }),
  previewExcelImport: (fileName: string, contentBase64: string, worksheetName: string | null) =>
    request<CsvImportPreview>('/api/imports/excel/preview', {
      method: 'POST',
      body: JSON.stringify({ fileName, contentBase64, worksheetName }),
    }),
  commitExcelImport: (fileName: string, contentBase64: string, worksheetName: string | null) =>
    request<ImportBatch>('/api/imports/excel/commit', {
      method: 'POST',
      body: JSON.stringify({ fileName, contentBase64, worksheetName }),
    }),
  getImportHistory: () => request<ImportBatch[]>('/api/imports'),
  getCategoryKeywordRules: () =>
    request<CategoryKeywordRule[]>('/api/category-keyword-rules'),
  createCategoryKeywordRule: (rule: CreateCategoryKeywordRuleRequest) =>
    request<CategoryKeywordRule>('/api/category-keyword-rules', {
      method: 'POST',
      body: JSON.stringify(rule),
    }),
  updateCategoryKeywordRule: (id: string, rule: UpdateCategoryKeywordRuleRequest) =>
    request<void>(`/api/category-keyword-rules/${id}`, {
      method: 'PUT',
      body: JSON.stringify(rule),
    }),
  deleteCategoryKeywordRule: (id: string) =>
    request<void>(`/api/category-keyword-rules/${id}`, {
      method: 'DELETE',
    }),
}
