import { lazy, Suspense, useEffect, useMemo, useState } from 'react'
import {
  ArrowDownCircle,
  ArrowUpCircle,
  CalendarDays,
  CheckCircle2,
  FileText,
  Loader2,
  Pencil,
  Plus,
  RefreshCcw,
  Trash2,
  Wallet,
  X,
} from 'lucide-react'
import { api } from './api'
import type {
  Account,
  AccountType,
  Category,
  CategoryType,
  CreateAccountRequest,
  CreateCategoryRequest,
  CreateInstallmentPurchaseRequest,
  CreateRecurringRuleRequest,
  CsvImportPreview,
  ForecastMonth,
  ImportBatch,
  CreateTransactionRequest,
  MonthlySummary,
  RecurringRule,
  RecurringFrequency,
  Transaction,
  TransactionType,
} from './api'

type TransactionFilters = {
  startDate: string
  endDate: string
  categoryId: string
  accountId: string
  type: TransactionType | ''
  isPaid: '' | 'true' | 'false'
}

const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const today = new Date()
const currentYear = today.getFullYear()
const currentMonth = today.getMonth() + 1
const todayText = today.toISOString().slice(0, 10)

const Charts = lazy(() => import('./components/Charts'))

const emptyTransactionForm: CreateTransactionRequest = {
  accountId: '',
  categoryId: '',
  description: '',
  amount: 0,
  type: 'Expense',
  date: todayText,
  dueDate: '',
  isPaid: false,
  paymentDate: '',
}

const emptyAccountForm: CreateAccountRequest = {
  name: '',
  type: 'BankAccount',
  initialBalance: 0,
}

const emptyCategoryForm: CreateCategoryRequest = {
  name: '',
  type: 'Expense',
  parentCategoryId: null,
}

const emptyInstallmentForm: CreateInstallmentPurchaseRequest = {
  accountId: '',
  categoryId: '',
  description: '',
  totalAmount: 0,
  totalInstallments: 2,
  startDate: todayText,
  dueDay: null,
}

const emptyRecurringForm: CreateRecurringRuleRequest = {
  accountId: '',
  categoryId: '',
  description: '',
  amount: 0,
  frequency: 'Monthly',
  dayOfMonth: Number(todayText.slice(8, 10)),
  startDate: todayText,
  endDate: null,
}

const initialFilters: TransactionFilters = {
  startDate: `${currentYear}-${String(currentMonth).padStart(2, '0')}-01`,
  endDate: new Date(currentYear, currentMonth, 0).toISOString().slice(0, 10),
  categoryId: '',
  accountId: '',
  type: '',
  isPaid: '',
}

function App() {
  const [summary, setSummary] = useState<MonthlySummary | null>(null)
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [recurringRules, setRecurringRules] = useState<RecurringRule[]>([])
  const [forecast, setForecast] = useState<ForecastMonth[]>([])
  const [importHistory, setImportHistory] = useState<ImportBatch[]>([])
  const [transactionForm, setTransactionForm] =
    useState<CreateTransactionRequest>(emptyTransactionForm)
  const [accountForm, setAccountForm] = useState<CreateAccountRequest>(emptyAccountForm)
  const [categoryForm, setCategoryForm] = useState<CreateCategoryRequest>(emptyCategoryForm)
  const [installmentForm, setInstallmentForm] =
    useState<CreateInstallmentPurchaseRequest>(emptyInstallmentForm)
  const [recurringForm, setRecurringForm] =
    useState<CreateRecurringRuleRequest>(emptyRecurringForm)
  const [csvFileName, setCsvFileName] = useState('import.csv')
  const [csvContent, setCsvContent] = useState('')
  const [csvPreview, setCsvPreview] = useState<CsvImportPreview | null>(null)
  const [filters, setFilters] = useState<TransactionFilters>(initialFilters)
  const [year, setYear] = useState(currentYear)
  const [month, setMonth] = useState(currentMonth)
  const [editingTransactionId, setEditingTransactionId] = useState<string | null>(null)
  const [editingAccountId, setEditingAccountId] = useState<string | null>(null)
  const [editingCategoryId, setEditingCategoryId] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSavingTransaction, setIsSavingTransaction] = useState(false)
  const [isSavingAccount, setIsSavingAccount] = useState(false)
  const [isSavingCategory, setIsSavingCategory] = useState(false)
  const [isSavingInstallment, setIsSavingInstallment] = useState(false)
  const [isSavingRecurring, setIsSavingRecurring] = useState(false)
  const [isImporting, setIsImporting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const accountById = useMemo(
    () => new Map(accounts.map((account) => [account.id, account])),
    [accounts],
  )

  const categoryById = useMemo(
    () => new Map(categories.map((category) => [category.id, category])),
    [categories],
  )

  const selectedCategories = useMemo(() => {
    return categories.filter(
      (category) =>
        category.type === 'Both' || category.type === transactionForm.type,
    )
  }, [categories, transactionForm.type])

  const expenseCategories = useMemo(
    () => categories.filter((category) => category.type === 'Expense' || category.type === 'Both'),
    [categories],
  )

  async function loadData() {
    setIsLoading(true)
    setError(null)

    try {
      const [
        summaryData,
        transactionsData,
        accountsData,
        categoriesData,
        recurringRulesData,
        forecastData,
        importHistoryData,
      ] =
        await Promise.all([
          api.getMonthlySummary(year, month),
          api.getTransactions({
            startDate: filters.startDate,
            endDate: filters.endDate,
            categoryId: filters.categoryId,
            accountId: filters.accountId,
            type: filters.type,
            isPaid: filters.isPaid === '' ? '' : filters.isPaid === 'true',
          }),
          api.getAccounts(),
          api.getCategories(),
          api.getRecurringRules(),
          api.getForecast(year, month, 6),
          api.getImportHistory(),
        ])

      setSummary(summaryData)
      setTransactions(transactionsData)
      setAccounts(accountsData)
      setCategories(categoriesData)
      setRecurringRules(recurringRulesData)
      setForecast(forecastData)
      setImportHistory(importHistoryData)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar dados.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [])

  function applyMonthToFilters() {
    const startDate = `${year}-${String(month).padStart(2, '0')}-01`
    const endDate = new Date(year, month, 0).toISOString().slice(0, 10)
    setFilters((current) => ({ ...current, startDate, endDate }))
  }

  async function handleSaveTransaction(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSavingTransaction(true)
    setError(null)

    try {
      const payload = {
        ...transactionForm,
        amount: Number(transactionForm.amount),
        dueDate: transactionForm.dueDate || null,
        paymentDate: transactionForm.isPaid
          ? transactionForm.paymentDate || transactionForm.date
          : null,
      }

      if (editingTransactionId) {
        await api.updateTransaction(editingTransactionId, payload)
      } else {
        await api.createTransaction(payload)
      }

      setTransactionForm({
        ...emptyTransactionForm,
        accountId: transactionForm.accountId,
        date: transactionForm.date,
      })
      setEditingTransactionId(null)
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar lancamento.')
    } finally {
      setIsSavingTransaction(false)
    }
  }

  async function handleSaveAccount(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSavingAccount(true)
    setError(null)

    try {
      const payload = {
        ...accountForm,
        initialBalance: Number(accountForm.initialBalance),
      }

      if (editingAccountId) {
        await api.updateAccount(editingAccountId, payload)
      } else {
        const account = await api.createAccount(payload)
        setTransactionForm((current) => ({ ...current, accountId: account.id }))
      }

      setAccountForm(emptyAccountForm)
      setEditingAccountId(null)
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar conta.')
    } finally {
      setIsSavingAccount(false)
    }
  }

  async function handleSaveCategory(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSavingCategory(true)
    setError(null)

    try {
      if (editingCategoryId) {
        await api.updateCategory(editingCategoryId, categoryForm)
      } else {
        const category = await api.createCategory(categoryForm)
        setTransactionForm((current) => ({
          ...current,
          categoryId: category.id,
          type: category.type === 'Income' ? 'Income' : current.type,
        }))
      }

      setCategoryForm(emptyCategoryForm)
      setEditingCategoryId(null)
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar categoria.')
    } finally {
      setIsSavingCategory(false)
    }
  }

  async function handleSaveInstallment(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSavingInstallment(true)
    setError(null)

    try {
      await api.createInstallmentPurchase({
        ...installmentForm,
        totalAmount: Number(installmentForm.totalAmount),
        totalInstallments: Number(installmentForm.totalInstallments),
        dueDay: installmentForm.dueDay ? Number(installmentForm.dueDay) : null,
      })
      setInstallmentForm({
        ...emptyInstallmentForm,
        accountId: installmentForm.accountId,
        categoryId: installmentForm.categoryId,
      })
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao gerar parcelas.')
    } finally {
      setIsSavingInstallment(false)
    }
  }

  async function handleSaveRecurring(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSavingRecurring(true)
    setError(null)

    try {
      await api.createRecurringRule({
        ...recurringForm,
        amount: Number(recurringForm.amount),
        dayOfMonth: Number(recurringForm.dayOfMonth),
        endDate: recurringForm.endDate || null,
      })
      setRecurringForm({
        ...emptyRecurringForm,
        accountId: recurringForm.accountId,
        categoryId: recurringForm.categoryId,
      })
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar recorrencia.')
    } finally {
      setIsSavingRecurring(false)
    }
  }

  async function generateRecurringTransactions() {
    const throughDate = new Date(year, month + 2, 0).toISOString().slice(0, 10)
    await runAction(
      async () => {
        const result = await api.generateRecurringTransactions(throughDate)
        window.alert(`${result.createdTransactions} lancamento(s) recorrente(s) gerado(s).`)
      },
      'Erro ao gerar recorrencias.',
    )
  }

  async function previewCsvImport() {
    setIsImporting(true)
    setError(null)

    try {
      const preview = await api.previewCsvImport(csvFileName, csvContent)
      setCsvPreview(preview)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao validar CSV.')
    } finally {
      setIsImporting(false)
    }
  }

  async function commitCsvImport() {
    setIsImporting(true)
    setError(null)

    try {
      await api.commitCsvImport(csvFileName, csvContent)
      setCsvContent('')
      setCsvPreview(null)
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao importar CSV.')
    } finally {
      setIsImporting(false)
    }
  }

  function editTransaction(transaction: Transaction) {
    setEditingTransactionId(transaction.id)
    setTransactionForm({
      accountId: transaction.accountId,
      categoryId: transaction.categoryId,
      description: transaction.description,
      amount: transaction.amount,
      type: transaction.type,
      date: transaction.date,
      dueDate: transaction.dueDate ?? '',
      isPaid: transaction.isPaid,
      paymentDate: transaction.paymentDate ?? '',
    })
  }

  function editAccount(account: Account) {
    setEditingAccountId(account.id)
    setAccountForm({
      name: account.name,
      type: account.type,
      initialBalance: account.initialBalance,
    })
  }

  function editCategory(category: Category) {
    setEditingCategoryId(category.id)
    setCategoryForm({
      name: category.name,
      type: category.type,
      parentCategoryId: category.parentCategoryId,
    })
  }

  async function deleteTransaction(transaction: Transaction) {
    if (!window.confirm(`Excluir o lancamento "${transaction.description}"?`)) {
      return
    }

    await runAction(() => api.deleteTransaction(transaction.id), 'Erro ao excluir lancamento.')
  }

  async function deleteAccount(account: Account) {
    if (!window.confirm(`Excluir a conta "${account.name}"?`)) {
      return
    }

    await runAction(() => api.deleteAccount(account.id), 'Erro ao excluir conta.')
  }

  async function deleteCategory(category: Category) {
    if (!window.confirm(`Excluir a categoria "${category.name}"?`)) {
      return
    }

    await runAction(() => api.deleteCategory(category.id), 'Erro ao excluir categoria.')
  }

  async function togglePaid(transaction: Transaction) {
    await runAction(
      () =>
        api.updateTransaction(transaction.id, {
          accountId: transaction.accountId,
          categoryId: transaction.categoryId,
          description: transaction.description,
          amount: transaction.amount,
          type: transaction.type,
          date: transaction.date,
          dueDate: transaction.dueDate,
          isPaid: !transaction.isPaid,
          paymentDate: transaction.isPaid ? null : todayText,
        }),
      'Erro ao atualizar pagamento.',
    )
  }

  async function runAction(action: () => Promise<void>, fallbackError: string) {
    setError(null)

    try {
      await action()
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : fallbackError)
    }
  }

  const hasBaseData = accounts.length > 0 && categories.length > 0

  return (
    <main className="min-h-screen bg-slate-50 text-slate-950">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-5 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm font-medium text-teal-700">FinTrack</p>
            <h1 className="text-2xl font-semibold tracking-tight">Dashboard financeiro</h1>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <select
              className="rounded-md border border-slate-300 bg-white px-3 py-2 text-sm"
              value={month}
              onChange={(event) => setMonth(Number(event.target.value))}
            >
              {Array.from({ length: 12 }, (_, index) => index + 1).map((value) => (
                <option key={value} value={value}>
                  {String(value).padStart(2, '0')}
                </option>
              ))}
            </select>
            <input
              className="w-24 rounded-md border border-slate-300 px-3 py-2 text-sm"
              type="number"
              value={year}
              onChange={(event) => setYear(Number(event.target.value))}
            />
            <button
              className="rounded-md border border-slate-300 px-3 py-2 text-sm font-medium hover:bg-slate-100"
              type="button"
              onClick={applyMonthToFilters}
            >
              Usar mes
            </button>
            <button
              className="inline-flex items-center gap-2 rounded-md bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800"
              type="button"
              onClick={() => void loadData()}
            >
              <RefreshCcw size={16} />
              Atualizar
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto grid max-w-7xl gap-6 px-4 py-6 lg:grid-cols-[1fr_390px]">
        <section className="space-y-6">
          {error && (
            <div className="rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
              {error}
            </div>
          )}

          {isLoading ? (
            <div className="flex h-64 items-center justify-center rounded-md border border-slate-200 bg-white">
              <Loader2 className="animate-spin text-slate-500" />
            </div>
          ) : (
            <>
              <SummaryCards summary={summary} />
              <Suspense
                fallback={
                  <div className="h-72 rounded-md border border-slate-200 bg-white p-4 shadow-sm" />
                }
              >
                <Charts summary={summary} />
              </Suspense>
              <ForecastPanel forecast={forecast} />
              <TransactionFiltersPanel
                accounts={accounts}
                categories={categories}
                filters={filters}
                onChange={setFilters}
                onSubmit={() => void loadData()}
              />
              <TransactionsTable
                accountById={accountById}
                categoryById={categoryById}
                onDelete={deleteTransaction}
                onEdit={editTransaction}
                onTogglePaid={togglePaid}
                transactions={transactions}
              />
              <ManageLists
                accounts={accounts}
                categories={categories}
                onDeleteAccount={deleteAccount}
                onDeleteCategory={deleteCategory}
                onEditAccount={editAccount}
                onEditCategory={editCategory}
              />
              <ImportPanel
                accounts={accounts}
                categories={categories}
                fileName={csvFileName}
                history={importHistory}
                isImporting={isImporting}
                onCommit={() => void commitCsvImport()}
                onContentChange={setCsvContent}
                onFileNameChange={setCsvFileName}
                onPreview={() => void previewCsvImport()}
                preview={csvPreview}
                value={csvContent}
              />
            </>
          )}
        </section>

        <aside className="space-y-4">
          <AccountForm
            form={accountForm}
            isEditing={Boolean(editingAccountId)}
            isSaving={isSavingAccount}
            onCancel={() => {
              setEditingAccountId(null)
              setAccountForm(emptyAccountForm)
            }}
            onChange={setAccountForm}
            onSubmit={handleSaveAccount}
          />

          <CategoryForm
            form={categoryForm}
            isEditing={Boolean(editingCategoryId)}
            isSaving={isSavingCategory}
            onCancel={() => {
              setEditingCategoryId(null)
              setCategoryForm(emptyCategoryForm)
            }}
            onChange={setCategoryForm}
            onSubmit={handleSaveCategory}
          />

          <InstallmentForm
            accounts={accounts}
            categories={expenseCategories}
            form={installmentForm}
            hasBaseData={hasBaseData}
            isSaving={isSavingInstallment}
            onChange={setInstallmentForm}
            onSubmit={handleSaveInstallment}
          />

          <RecurringForm
            accounts={accounts}
            categories={expenseCategories}
            form={recurringForm}
            hasBaseData={hasBaseData}
            isSaving={isSavingRecurring}
            onChange={setRecurringForm}
            onGenerate={() => void generateRecurringTransactions()}
            onSubmit={handleSaveRecurring}
            rules={recurringRules}
          />

          <TransactionForm
            accounts={accounts}
            categories={selectedCategories}
            form={transactionForm}
            hasBaseData={hasBaseData}
            isEditing={Boolean(editingTransactionId)}
            isSaving={isSavingTransaction}
            onCancel={() => {
              setEditingTransactionId(null)
              setTransactionForm(emptyTransactionForm)
            }}
            onChange={setTransactionForm}
            onSubmit={handleSaveTransaction}
          />
        </aside>
      </div>
    </main>
  )
}

function SummaryCards({ summary }: { summary: MonthlySummary | null }) {
  const cards = [
    { label: 'Receitas', value: summary?.totalIncome ?? 0, icon: ArrowUpCircle, className: 'text-emerald-700' },
    { label: 'Despesas', value: summary?.totalExpense ?? 0, icon: ArrowDownCircle, className: 'text-red-700' },
    { label: 'Saldo', value: summary?.balance ?? 0, icon: Wallet, className: 'text-slate-900' },
    { label: 'Pendentes', value: summary?.unpaidExpenses ?? 0, icon: CalendarDays, className: 'text-amber-700' },
  ]

  return (
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      {cards.map((card) => {
        const Icon = card.icon
        return (
          <div key={card.label} className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
            <div className="mb-3 flex items-center justify-between">
              <p className="text-sm font-medium text-slate-500">{card.label}</p>
              <Icon className={card.className} size={20} />
            </div>
            <p className="text-2xl font-semibold">{currency.format(card.value)}</p>
          </div>
        )
      })}
    </div>
  )
}

function ForecastPanel({ forecast }: { forecast: ForecastMonth[] }) {
  return (
    <div className="overflow-hidden rounded-md border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-4 py-3">
        <h2 className="font-semibold">Previsao dos proximos meses</h2>
      </div>
      <div className="grid gap-px bg-slate-200 sm:grid-cols-2 xl:grid-cols-3">
        {forecast.map((month) => (
          <div key={`${month.year}-${month.month}`} className="bg-white p-4">
            <p className="text-sm font-medium text-slate-500">
              {String(month.month).padStart(2, '0')}/{month.year}
            </p>
            <p className={`mt-2 text-xl font-semibold ${month.balance >= 0 ? 'text-emerald-700' : 'text-red-700'}`}>
              {currency.format(month.balance)}
            </p>
            <div className="mt-3 space-y-1 text-xs text-slate-500">
              <p>Receitas: {currency.format(month.income)}</p>
              <p>Despesas: {currency.format(month.expense)}</p>
              <p>Recorrencias previstas: {currency.format(month.projectedRecurringExpenses)}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

function TransactionFiltersPanel({
  accounts,
  categories,
  filters,
  onChange,
  onSubmit,
}: {
  accounts: Account[]
  categories: Category[]
  filters: TransactionFilters
  onChange: (filters: TransactionFilters) => void
  onSubmit: () => void
}) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-semibold">Filtros de lancamentos</h2>
        <button
          className="rounded-md bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800"
          type="button"
          onClick={onSubmit}
        >
          Filtrar
        </button>
      </div>
      <div className="grid gap-3 md:grid-cols-3 xl:grid-cols-6">
        <Input label="Inicio" type="date" value={filters.startDate} onChange={(value) => onChange({ ...filters, startDate: value })} />
        <Input label="Fim" type="date" value={filters.endDate} onChange={(value) => onChange({ ...filters, endDate: value })} />
        <Select label="Tipo" value={filters.type} onChange={(value) => onChange({ ...filters, type: value as TransactionType | '' })}>
          <option value="">Todos</option>
          <option value="Income">Receita</option>
          <option value="Expense">Despesa</option>
        </Select>
        <Select label="Status" value={filters.isPaid} onChange={(value) => onChange({ ...filters, isPaid: value as TransactionFilters['isPaid'] })}>
          <option value="">Todos</option>
          <option value="true">Pago</option>
          <option value="false">Pendente</option>
        </Select>
        <Select label="Conta" value={filters.accountId} onChange={(value) => onChange({ ...filters, accountId: value })}>
          <option value="">Todas</option>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>{account.name}</option>
          ))}
        </Select>
        <Select label="Categoria" value={filters.categoryId} onChange={(value) => onChange({ ...filters, categoryId: value })}>
          <option value="">Todas</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>{category.name}</option>
          ))}
        </Select>
      </div>
    </div>
  )
}

function TransactionsTable({
  accountById,
  categoryById,
  onDelete,
  onEdit,
  onTogglePaid,
  transactions,
}: {
  accountById: Map<string, Account>
  categoryById: Map<string, Category>
  onDelete: (transaction: Transaction) => void
  onEdit: (transaction: Transaction) => void
  onTogglePaid: (transaction: Transaction) => void
  transactions: Transaction[]
}) {
  return (
    <div className="overflow-hidden rounded-md border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-4 py-3">
        <h2 className="font-semibold">Lancamentos</h2>
      </div>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50 text-left text-slate-500">
            <tr>
              <th className="px-4 py-3 font-medium">Data</th>
              <th className="px-4 py-3 font-medium">Descricao</th>
              <th className="px-4 py-3 font-medium">Conta</th>
              <th className="px-4 py-3 font-medium">Categoria</th>
              <th className="px-4 py-3 text-right font-medium">Valor</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 text-right font-medium">Acoes</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {transactions.length === 0 ? (
              <tr>
                <td className="px-4 py-8 text-center text-slate-500" colSpan={7}>
                  Nenhum lancamento encontrado.
                </td>
              </tr>
            ) : (
              transactions.map((transaction) => (
                <tr key={transaction.id}>
                  <td className="whitespace-nowrap px-4 py-3">{transaction.date}</td>
                  <td className="min-w-52 px-4 py-3 font-medium text-slate-800">{transaction.description}</td>
                  <td className="px-4 py-3">{accountById.get(transaction.accountId)?.name ?? '-'}</td>
                  <td className="px-4 py-3">{categoryById.get(transaction.categoryId)?.name ?? '-'}</td>
                  <td className={`whitespace-nowrap px-4 py-3 text-right font-semibold ${transaction.type === 'Income' ? 'text-emerald-700' : 'text-red-700'}`}>
                    {currency.format(transaction.amount)}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-1 text-xs font-medium ${transaction.isPaid ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700'}`}>
                      {transaction.isPaid ? 'Pago' : 'Pendente'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <ActionButtons
                      onDelete={() => onDelete(transaction)}
                      onEdit={() => onEdit(transaction)}
                      onToggle={() => onTogglePaid(transaction)}
                      toggleTitle={transaction.isPaid ? 'Marcar como pendente' : 'Marcar como pago'}
                    />
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function ManageLists({
  accounts,
  categories,
  onDeleteAccount,
  onDeleteCategory,
  onEditAccount,
  onEditCategory,
}: {
  accounts: Account[]
  categories: Category[]
  onDeleteAccount: (account: Account) => void
  onDeleteCategory: (category: Category) => void
  onEditAccount: (account: Account) => void
  onEditCategory: (category: Category) => void
}) {
  return (
    <div className="grid gap-4 xl:grid-cols-2">
      <SimpleList
        emptyText="Nenhuma conta cadastrada."
        items={accounts}
        title="Contas"
        renderMeta={(account) => `${account.type} - ${currency.format(account.initialBalance)}`}
        onDelete={onDeleteAccount}
        onEdit={onEditAccount}
      />
      <SimpleList
        emptyText="Nenhuma categoria cadastrada."
        items={categories}
        title="Categorias"
        renderMeta={(category) => category.type}
        onDelete={onDeleteCategory}
        onEdit={onEditCategory}
      />
    </div>
  )
}

function ImportPanel({
  accounts,
  categories,
  fileName,
  history,
  isImporting,
  onCommit,
  onContentChange,
  onFileNameChange,
  onPreview,
  preview,
  value,
}: {
  accounts: Account[]
  categories: Category[]
  fileName: string
  history: ImportBatch[]
  isImporting: boolean
  onCommit: () => void
  onContentChange: (value: string) => void
  onFileNameChange: (value: string) => void
  onPreview: () => void
  preview: CsvImportPreview | null
  value: string
}) {
  const accountById = new Map(accounts.map((account) => [account.id, account.name]))
  const categoryById = new Map(categories.map((category) => [category.id, category.name]))
  const canCommit = Boolean(preview && preview.validRows > 0)

  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <div className="mb-4 flex items-center gap-2">
        <FileText className="text-teal-700" size={18} />
        <h2 className="font-semibold">Importacao CSV</h2>
      </div>
      <div className="grid gap-4 xl:grid-cols-[1fr_320px]">
        <div className="space-y-3">
          <Input label="Nome do arquivo" value={fileName} onChange={onFileNameChange} />
          <TextArea
            label="CSV"
            onChange={onContentChange}
            placeholder="description,amount,type,date,accountId,categoryId,dueDate,isPaid,paymentDate"
            value={value}
          />
          <div className="flex flex-wrap gap-2">
            <button
              className="rounded-md border border-slate-300 px-3 py-2 text-sm font-medium hover:bg-slate-100 disabled:cursor-not-allowed disabled:bg-slate-100"
              disabled={isImporting || !value.trim()}
              type="button"
              onClick={onPreview}
            >
              Preview
            </button>
            <button
              className="rounded-md bg-teal-700 px-3 py-2 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:bg-slate-400"
              disabled={isImporting || !canCommit}
              type="button"
              onClick={onCommit}
            >
              Importar validos
            </button>
          </div>
          {preview && (
            <div className="rounded-md border border-slate-200">
              <div className="border-b border-slate-200 px-3 py-2 text-sm text-slate-600">
                {preview.validRows} validas, {preview.invalidRows} invalidas de {preview.totalRows}
              </div>
              <div className="max-h-72 overflow-auto">
                <table className="min-w-full divide-y divide-slate-100 text-xs">
                  <tbody className="divide-y divide-slate-100">
                    {preview.rows.map((row) => (
                      <tr key={row.rowNumber}>
                        <td className="whitespace-nowrap px-3 py-2">Linha {row.rowNumber}</td>
                        <td className="px-3 py-2">{row.description || '-'}</td>
                        <td className="whitespace-nowrap px-3 py-2">{row.amount ? currency.format(row.amount) : '-'}</td>
                        <td className="px-3 py-2">{row.accountId ? accountById.get(row.accountId) ?? row.accountId : '-'}</td>
                        <td className="px-3 py-2">{row.categoryId ? categoryById.get(row.categoryId) ?? row.categoryId : '-'}</td>
                        <td className={`px-3 py-2 ${row.errors.length ? 'text-red-700' : 'text-emerald-700'}`}>
                          {row.errors.length ? row.errors.join(' ') : 'OK'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
        <div>
          <h3 className="mb-2 text-sm font-semibold">Historico</h3>
          {history.length === 0 ? (
            <p className="text-sm text-slate-500">Nenhuma importacao realizada.</p>
          ) : (
            <div className="space-y-2">
              {history.map((batch) => (
                <div key={batch.id} className="rounded-md border border-slate-100 px-3 py-2 text-sm">
                  <p className="font-medium">{batch.fileName}</p>
                  <p className="text-xs text-slate-500">
                    {batch.status} - {batch.successRows} sucesso, {batch.failedRows} falha
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

function SimpleList<T extends { id: string; name: string }>({
  emptyText,
  items,
  onDelete,
  onEdit,
  renderMeta,
  title,
}: {
  emptyText: string
  items: T[]
  onDelete: (item: T) => void
  onEdit: (item: T) => void
  renderMeta: (item: T) => string
  title: string
}) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <h2 className="mb-3 font-semibold">{title}</h2>
      {items.length === 0 ? (
        <p className="text-sm text-slate-500">{emptyText}</p>
      ) : (
        <div className="space-y-2">
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between gap-3 rounded-md border border-slate-100 px-3 py-2">
              <div>
                <p className="text-sm font-medium">{item.name}</p>
                <p className="text-xs text-slate-500">{renderMeta(item)}</p>
              </div>
              <ActionButtons
                compact
                onDelete={() => onDelete(item)}
                onEdit={() => onEdit(item)}
              />
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function AccountForm({
  form,
  isEditing,
  isSaving,
  onCancel,
  onChange,
  onSubmit,
}: {
  form: CreateAccountRequest
  isEditing: boolean
  isSaving: boolean
  onCancel: () => void
  onChange: (form: CreateAccountRequest) => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
}) {
  return (
    <FormShell icon={<Wallet size={18} />} isEditing={isEditing} onCancel={onCancel} title={isEditing ? 'Editar conta' : 'Nova conta'}>
      <form className="space-y-3" onSubmit={onSubmit}>
        <Input label="Nome" value={form.name} onChange={(value) => onChange({ ...form, name: value })} placeholder="Nubank, carteira..." required />
        <div className="grid grid-cols-2 gap-3">
          <Select label="Tipo" value={form.type} onChange={(value) => onChange({ ...form, type: value as AccountType })}>
            <option value="BankAccount">Banco</option>
            <option value="CreditCard">Cartao</option>
            <option value="Cash">Dinheiro</option>
            <option value="Investment">Investimento</option>
          </Select>
          <Input label="Saldo inicial" type="number" step="0.01" value={String(form.initialBalance)} onChange={(value) => onChange({ ...form, initialBalance: Number(value) })} />
        </div>
        <SubmitButton isSaving={isSaving} text={isEditing ? 'Salvar conta' : 'Criar conta'} />
      </form>
    </FormShell>
  )
}

function CategoryForm({
  form,
  isEditing,
  isSaving,
  onCancel,
  onChange,
  onSubmit,
}: {
  form: CreateCategoryRequest
  isEditing: boolean
  isSaving: boolean
  onCancel: () => void
  onChange: (form: CreateCategoryRequest) => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
}) {
  return (
    <FormShell icon={<Plus size={18} />} isEditing={isEditing} onCancel={onCancel} title={isEditing ? 'Editar categoria' : 'Nova categoria'}>
      <form className="space-y-3" onSubmit={onSubmit}>
        <Input label="Nome" value={form.name} onChange={(value) => onChange({ ...form, name: value })} placeholder="Mercado, salario..." required />
        <Select label="Tipo" value={form.type} onChange={(value) => onChange({ ...form, type: value as CategoryType })}>
          <option value="Expense">Despesa</option>
          <option value="Income">Receita</option>
          <option value="Both">Ambos</option>
        </Select>
        <SubmitButton isSaving={isSaving} text={isEditing ? 'Salvar categoria' : 'Criar categoria'} />
      </form>
    </FormShell>
  )
}

function InstallmentForm({
  accounts,
  categories,
  form,
  hasBaseData,
  isSaving,
  onChange,
  onSubmit,
}: {
  accounts: Account[]
  categories: Category[]
  form: CreateInstallmentPurchaseRequest
  hasBaseData: boolean
  isSaving: boolean
  onChange: (form: CreateInstallmentPurchaseRequest) => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
}) {
  return (
    <FormShell icon={<CalendarDays size={18} />} isEditing={false} onCancel={() => undefined} title="Compra parcelada">
      <form className="space-y-3" onSubmit={onSubmit}>
        <Input label="Descricao" value={form.description} onChange={(value) => onChange({ ...form, description: value })} placeholder="Notebook, viagem..." required />
        <div className="grid grid-cols-2 gap-3">
          <Input label="Valor total" type="number" min="0.01" step="0.01" value={form.totalAmount ? String(form.totalAmount) : ''} onChange={(value) => onChange({ ...form, totalAmount: Number(value) })} required />
          <Input label="Parcelas" type="number" min="2" value={String(form.totalInstallments)} onChange={(value) => onChange({ ...form, totalInstallments: Number(value) })} required />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <Input label="Primeira parcela" type="date" value={form.startDate} onChange={(value) => onChange({ ...form, startDate: value })} required />
          <Input label="Dia venc." type="number" min="1" max="31" value={form.dueDay ? String(form.dueDay) : ''} onChange={(value) => onChange({ ...form, dueDay: value ? Number(value) : null })} />
        </div>
        <Select label="Conta" value={form.accountId} onChange={(value) => onChange({ ...form, accountId: value })} required>
          <option value="">Selecione</option>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>{account.name}</option>
          ))}
        </Select>
        <Select label="Categoria" value={form.categoryId} onChange={(value) => onChange({ ...form, categoryId: value })} required>
          <option value="">Selecione</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>{category.name}</option>
          ))}
        </Select>
        <SubmitButton disabled={!hasBaseData} isSaving={isSaving} text="Gerar parcelas" />
      </form>
    </FormShell>
  )
}

function RecurringForm({
  accounts,
  categories,
  form,
  hasBaseData,
  isSaving,
  onChange,
  onGenerate,
  onSubmit,
  rules,
}: {
  accounts: Account[]
  categories: Category[]
  form: CreateRecurringRuleRequest
  hasBaseData: boolean
  isSaving: boolean
  onChange: (form: CreateRecurringRuleRequest) => void
  onGenerate: () => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
  rules: RecurringRule[]
}) {
  return (
    <FormShell icon={<RefreshCcw size={18} />} isEditing={false} onCancel={() => undefined} title="Gasto recorrente">
      <form className="space-y-3" onSubmit={onSubmit}>
        <Input label="Descricao" value={form.description} onChange={(value) => onChange({ ...form, description: value })} placeholder="Aluguel, assinatura..." required />
        <div className="grid grid-cols-2 gap-3">
          <Input label="Valor" type="number" min="0.01" step="0.01" value={form.amount ? String(form.amount) : ''} onChange={(value) => onChange({ ...form, amount: Number(value) })} required />
          <Select label="Frequencia" value={form.frequency} onChange={(value) => onChange({ ...form, frequency: value as RecurringFrequency })}>
            <option value="Monthly">Mensal</option>
            <option value="Weekly">Semanal</option>
            <option value="Yearly">Anual</option>
          </Select>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <Input label="Inicio" type="date" value={form.startDate} onChange={(value) => onChange({ ...form, startDate: value })} required />
          <Input label="Dia" type="number" min="1" max="31" value={String(form.dayOfMonth)} onChange={(value) => onChange({ ...form, dayOfMonth: Number(value) })} required />
        </div>
        <Input label="Fim opcional" type="date" value={form.endDate ?? ''} onChange={(value) => onChange({ ...form, endDate: value || null })} />
        <Select label="Conta" value={form.accountId} onChange={(value) => onChange({ ...form, accountId: value })} required>
          <option value="">Selecione</option>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>{account.name}</option>
          ))}
        </Select>
        <Select label="Categoria" value={form.categoryId} onChange={(value) => onChange({ ...form, categoryId: value })} required>
          <option value="">Selecione</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>{category.name}</option>
          ))}
        </Select>
        <SubmitButton disabled={!hasBaseData} isSaving={isSaving} text="Salvar recorrencia" />
      </form>
      <div className="mt-4 border-t border-slate-100 pt-3">
        <button
          className="mb-3 w-full rounded-md border border-slate-300 px-3 py-2 text-sm font-medium hover:bg-slate-100"
          type="button"
          onClick={onGenerate}
        >
          Gerar proximos 3 meses
        </button>
        {rules.length > 0 && (
          <div className="space-y-2">
            {rules.slice(0, 4).map((rule) => (
              <div key={rule.id} className="rounded-md bg-slate-50 px-3 py-2 text-sm">
                <p className="font-medium">{rule.description}</p>
                <p className="text-xs text-slate-500">
                  {currency.format(rule.amount)} - {rule.frequency}
                </p>
              </div>
            ))}
          </div>
        )}
      </div>
    </FormShell>
  )
}

function TransactionForm({
  accounts,
  categories,
  form,
  hasBaseData,
  isEditing,
  isSaving,
  onCancel,
  onChange,
  onSubmit,
}: {
  accounts: Account[]
  categories: Category[]
  form: CreateTransactionRequest
  hasBaseData: boolean
  isEditing: boolean
  isSaving: boolean
  onCancel: () => void
  onChange: (form: CreateTransactionRequest) => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
}) {
  return (
    <FormShell icon={<Plus size={18} />} isEditing={isEditing} onCancel={onCancel} title={isEditing ? 'Editar lancamento' : 'Novo lancamento'}>
      {!hasBaseData && (
        <div className="mb-4 rounded-md bg-amber-50 p-3 text-sm text-amber-800">
          Cadastre ao menos uma conta e uma categoria antes de lancar transacoes.
        </div>
      )}
      <form className="space-y-3" onSubmit={onSubmit}>
        <Select label="Tipo" value={form.type} onChange={(value) => onChange({ ...form, type: value as TransactionType, categoryId: '' })}>
          <option value="Expense">Despesa</option>
          <option value="Income">Receita</option>
        </Select>
        <Input label="Descricao" value={form.description} onChange={(value) => onChange({ ...form, description: value })} placeholder="Mercado, salario..." required />
        <div className="grid grid-cols-2 gap-3">
          <Input label="Valor" type="number" min="0.01" step="0.01" value={form.amount ? String(form.amount) : ''} onChange={(value) => onChange({ ...form, amount: Number(value) })} required />
          <Input label="Data" type="date" value={form.date} onChange={(value) => onChange({ ...form, date: value })} required />
        </div>
        <Select label="Conta" value={form.accountId} onChange={(value) => onChange({ ...form, accountId: value })} required>
          <option value="">Selecione</option>
          {accounts.map((account) => (
            <option key={account.id} value={account.id}>{account.name}</option>
          ))}
        </Select>
        <Select label="Categoria" value={form.categoryId} onChange={(value) => onChange({ ...form, categoryId: value })} required>
          <option value="">Selecione</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>{category.name}</option>
          ))}
        </Select>
        <div className="grid grid-cols-2 gap-3">
          <Input label="Vencimento" type="date" value={form.dueDate ?? ''} onChange={(value) => onChange({ ...form, dueDate: value })} />
          <label className="flex items-end gap-2 pb-2 text-sm font-medium text-slate-700">
            <input
              checked={form.isPaid}
              className="h-4 w-4 rounded border-slate-300"
              type="checkbox"
              onChange={(event) => onChange({ ...form, isPaid: event.target.checked, paymentDate: event.target.checked ? form.date : '' })}
            />
            Pago
          </label>
        </div>
        {form.isPaid && (
          <Input label="Data de pagamento" type="date" value={form.paymentDate ?? ''} onChange={(value) => onChange({ ...form, paymentDate: value })} required />
        )}
        <SubmitButton disabled={!hasBaseData} isSaving={isSaving} text={isEditing ? 'Salvar alteracoes' : 'Salvar lancamento'} />
      </form>
    </FormShell>
  )
}

function FormShell({
  children,
  icon,
  isEditing,
  onCancel,
  title,
}: {
  children: React.ReactNode
  icon: React.ReactNode
  isEditing: boolean
  onCancel: () => void
  title: string
}) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-lg font-semibold">{title}</h2>
        {isEditing ? (
          <button className="rounded-md p-1 text-slate-500 hover:bg-slate-100" type="button" onClick={onCancel} title="Cancelar edicao">
            <X size={18} />
          </button>
        ) : (
          <span className="text-teal-700">{icon}</span>
        )}
      </div>
      {children}
    </div>
  )
}

function Input({
  label,
  onChange,
  value,
  ...props
}: Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> & {
  label: string
  onChange: (value: string) => void
  value: string
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block font-medium text-slate-700">{label}</span>
      <input
        {...props}
        className="w-full rounded-md border border-slate-300 px-3 py-2"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

function TextArea({
  label,
  onChange,
  value,
  ...props
}: Omit<React.TextareaHTMLAttributes<HTMLTextAreaElement>, 'onChange' | 'value'> & {
  label: string
  onChange: (value: string) => void
  value: string
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block font-medium text-slate-700">{label}</span>
      <textarea
        {...props}
        className="min-h-36 w-full rounded-md border border-slate-300 px-3 py-2 font-mono text-xs"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </label>
  )
}

function Select({
  children,
  label,
  onChange,
  value,
  ...props
}: Omit<React.SelectHTMLAttributes<HTMLSelectElement>, 'onChange' | 'value'> & {
  label: string
  onChange: (value: string) => void
  value: string
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block font-medium text-slate-700">{label}</span>
      <select
        {...props}
        className="w-full rounded-md border border-slate-300 bg-white px-3 py-2"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        {children}
      </select>
    </label>
  )
}

function SubmitButton({
  disabled,
  isSaving,
  text,
}: {
  disabled?: boolean
  isSaving: boolean
  text: string
}) {
  return (
    <button
      className="w-full rounded-md bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:bg-slate-400"
      disabled={disabled || isSaving}
      type="submit"
    >
      {isSaving ? 'Salvando...' : text}
    </button>
  )
}

function ActionButtons({
  compact,
  onDelete,
  onEdit,
  onToggle,
  toggleTitle,
}: {
  compact?: boolean
  onDelete: () => void
  onEdit: () => void
  onToggle?: () => void
  toggleTitle?: string
}) {
  return (
    <div className="flex justify-end gap-1">
      {onToggle && (
        <IconButton title={toggleTitle ?? 'Alternar status'} onClick={onToggle}>
          <CheckCircle2 size={compact ? 15 : 16} />
        </IconButton>
      )}
      <IconButton title="Editar" onClick={onEdit}>
        <Pencil size={compact ? 15 : 16} />
      </IconButton>
      <IconButton danger title="Excluir" onClick={onDelete}>
        <Trash2 size={compact ? 15 : 16} />
      </IconButton>
    </div>
  )
}

function IconButton({
  children,
  danger,
  onClick,
  title,
}: {
  children: React.ReactNode
  danger?: boolean
  onClick: () => void
  title: string
}) {
  return (
    <button
      className={`rounded-md p-1.5 text-slate-500 ${
        danger
          ? 'hover:bg-red-50 hover:text-red-700'
          : 'hover:bg-slate-100 hover:text-slate-900'
      }`}
      type="button"
      onClick={onClick}
      title={title}
    >
      {children}
    </button>
  )
}

export default App
