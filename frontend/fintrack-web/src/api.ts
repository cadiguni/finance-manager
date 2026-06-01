export type AccountType = 'BankAccount' | 'CreditCard' | 'Cash' | 'Investment'
export type CategoryType = 'Income' | 'Expense' | 'Both'
export type TransactionType = 'Income' | 'Expense'

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

type TransactionFilters = {
  startDate?: string
  endDate?: string
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
  getMonthlySummary: (year: number, month: number) =>
    request<MonthlySummary>(`/api/dashboard/monthly-summary${toQueryString({ year, month })}`),
  getTransactions: (filters: TransactionFilters = {}) =>
    request<Transaction[]>(`/api/transactions${toQueryString(filters)}`),
  createTransaction: (transaction: CreateTransactionRequest) =>
    request<Transaction>('/api/transactions', {
      method: 'POST',
      body: JSON.stringify(transaction),
    }),
}
