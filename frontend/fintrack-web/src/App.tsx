import { useEffect, useMemo, useState } from 'react'
import {
  ArrowDownCircle,
  ArrowUpCircle,
  Banknote,
  CalendarDays,
  Loader2,
  Plus,
  RefreshCcw,
  Wallet,
} from 'lucide-react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import {
  api,
} from './api'
import type {
  Account,
  Category,
  CreateTransactionRequest,
  MonthlySummary,
  Transaction,
} from './api'

type TransactionType = 'Income' | 'Expense'

const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const currentDate = new Date()
const currentYear = currentDate.getFullYear()
const currentMonth = currentDate.getMonth() + 1

const colors = ['#0f766e', '#2563eb', '#7c3aed', '#db2777', '#ea580c', '#65a30d']

const emptyForm: CreateTransactionRequest = {
  accountId: '',
  categoryId: '',
  description: '',
  amount: 0,
  type: 'Expense',
  date: currentDate.toISOString().slice(0, 10),
  dueDate: '',
  isPaid: false,
  paymentDate: '',
}

function App() {
  const [summary, setSummary] = useState<MonthlySummary | null>(null)
  const [transactions, setTransactions] = useState<Transaction[]>([])
  const [accounts, setAccounts] = useState<Account[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [form, setForm] = useState<CreateTransactionRequest>(emptyForm)
  const [year, setYear] = useState(currentYear)
  const [month, setMonth] = useState(currentMonth)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const selectedCategories = useMemo(() => {
    return categories.filter(
      (category) => category.type === 'Both' || category.type === form.type,
    )
  }, [categories, form.type])

  async function loadData() {
    setIsLoading(true)
    setError(null)

    try {
      const startDate = `${year}-${String(month).padStart(2, '0')}-01`
      const endDate = new Date(year, month, 0).toISOString().slice(0, 10)
      const [summaryData, transactionsData, accountsData, categoriesData] =
        await Promise.all([
          api.getMonthlySummary(year, month),
          api.getTransactions({ startDate, endDate }),
          api.getAccounts(),
          api.getCategories(),
        ])

      setSummary(summaryData)
      setTransactions(transactionsData)
      setAccounts(accountsData)
      setCategories(categoriesData)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar dados.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [year, month])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)
    setError(null)

    try {
      await api.createTransaction({
        ...form,
        amount: Number(form.amount),
        dueDate: form.dueDate || null,
        paymentDate: form.isPaid ? form.paymentDate || form.date : null,
      })

      setForm({
        ...emptyForm,
        accountId: form.accountId,
        categoryId: '',
        date: form.date,
      })
      await loadData()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar lançamento.')
    } finally {
      setIsSaving(false)
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

      <div className="mx-auto grid max-w-7xl gap-6 px-4 py-6 lg:grid-cols-[1fr_380px]">
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
              <Charts summary={summary} />
              <TransactionsTable transactions={transactions} />
            </>
          )}
        </section>

        <aside className="space-y-4">
          <form
            className="rounded-md border border-slate-200 bg-white p-4 shadow-sm"
            onSubmit={(event) => void handleSubmit(event)}
          >
            <div className="mb-4 flex items-center justify-between">
              <h2 className="text-lg font-semibold">Novo lançamento</h2>
              <Plus size={18} className="text-teal-700" />
            </div>

            {!hasBaseData && (
              <div className="mb-4 rounded-md bg-amber-50 p-3 text-sm text-amber-800">
                Cadastre ao menos uma conta e uma categoria pela API antes de lançar
                transações.
              </div>
            )}

            <div className="space-y-3">
              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Tipo</span>
                <select
                  className="w-full rounded-md border border-slate-300 px-3 py-2"
                  value={form.type}
                  onChange={(event) =>
                    setForm({
                      ...form,
                      type: event.target.value as TransactionType,
                      categoryId: '',
                    })
                  }
                >
                  <option value="Expense">Despesa</option>
                  <option value="Income">Receita</option>
                </select>
              </label>

              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Descrição</span>
                <input
                  className="w-full rounded-md border border-slate-300 px-3 py-2"
                  value={form.description}
                  onChange={(event) => setForm({ ...form, description: event.target.value })}
                  placeholder="Mercado, salário, aluguel..."
                  required
                />
              </label>

              <div className="grid grid-cols-2 gap-3">
                <label className="block text-sm">
                  <span className="mb-1 block font-medium text-slate-700">Valor</span>
                  <input
                    className="w-full rounded-md border border-slate-300 px-3 py-2"
                    min="0.01"
                    step="0.01"
                    type="number"
                    value={form.amount || ''}
                    onChange={(event) =>
                      setForm({ ...form, amount: Number(event.target.value) })
                    }
                    required
                  />
                </label>

                <label className="block text-sm">
                  <span className="mb-1 block font-medium text-slate-700">Data</span>
                  <input
                    className="w-full rounded-md border border-slate-300 px-3 py-2"
                    type="date"
                    value={form.date}
                    onChange={(event) => setForm({ ...form, date: event.target.value })}
                    required
                  />
                </label>
              </div>

              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Conta</span>
                <select
                  className="w-full rounded-md border border-slate-300 px-3 py-2"
                  value={form.accountId}
                  onChange={(event) => setForm({ ...form, accountId: event.target.value })}
                  required
                >
                  <option value="">Selecione</option>
                  {accounts.map((account) => (
                    <option key={account.id} value={account.id}>
                      {account.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="block text-sm">
                <span className="mb-1 block font-medium text-slate-700">Categoria</span>
                <select
                  className="w-full rounded-md border border-slate-300 px-3 py-2"
                  value={form.categoryId}
                  onChange={(event) => setForm({ ...form, categoryId: event.target.value })}
                  required
                >
                  <option value="">Selecione</option>
                  {selectedCategories.map((category) => (
                    <option key={category.id} value={category.id}>
                      {category.name}
                    </option>
                  ))}
                </select>
              </label>

              <div className="grid grid-cols-2 gap-3">
                <label className="block text-sm">
                  <span className="mb-1 block font-medium text-slate-700">Vencimento</span>
                  <input
                    className="w-full rounded-md border border-slate-300 px-3 py-2"
                    type="date"
                    value={form.dueDate ?? ''}
                    onChange={(event) => setForm({ ...form, dueDate: event.target.value })}
                  />
                </label>

                <label className="flex items-end gap-2 pb-2 text-sm font-medium text-slate-700">
                  <input
                    checked={form.isPaid}
                    className="h-4 w-4 rounded border-slate-300"
                    type="checkbox"
                    onChange={(event) =>
                      setForm({
                        ...form,
                        isPaid: event.target.checked,
                        paymentDate: event.target.checked ? form.date : '',
                      })
                    }
                  />
                  Pago
                </label>
              </div>

              {form.isPaid && (
                <label className="block text-sm">
                  <span className="mb-1 block font-medium text-slate-700">Data de pagamento</span>
                  <input
                    className="w-full rounded-md border border-slate-300 px-3 py-2"
                    type="date"
                    value={form.paymentDate ?? ''}
                    onChange={(event) =>
                      setForm({ ...form, paymentDate: event.target.value })
                    }
                    required
                  />
                </label>
              )}

              <button
                className="w-full rounded-md bg-teal-700 px-4 py-2.5 text-sm font-semibold text-white hover:bg-teal-800 disabled:cursor-not-allowed disabled:bg-slate-400"
                disabled={isSaving || !hasBaseData}
                type="submit"
              >
                {isSaving ? 'Salvando...' : 'Salvar lançamento'}
              </button>
            </div>
          </form>
        </aside>
      </div>
    </main>
  )
}

function SummaryCards({ summary }: { summary: MonthlySummary | null }) {
  const cards = [
    {
      label: 'Receitas',
      value: summary?.totalIncome ?? 0,
      icon: ArrowUpCircle,
      className: 'text-emerald-700',
    },
    {
      label: 'Despesas',
      value: summary?.totalExpense ?? 0,
      icon: ArrowDownCircle,
      className: 'text-red-700',
    },
    {
      label: 'Saldo',
      value: summary?.balance ?? 0,
      icon: Wallet,
      className: 'text-slate-900',
    },
    {
      label: 'Pendentes',
      value: summary?.unpaidExpenses ?? 0,
      icon: CalendarDays,
      className: 'text-amber-700',
    },
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

function Charts({ summary }: { summary: MonthlySummary | null }) {
  const cashFlow = [
    { name: 'Receitas', total: summary?.totalIncome ?? 0 },
    { name: 'Despesas', total: summary?.totalExpense ?? 0 },
  ]

  const categoryData = summary?.expensesByCategory ?? []

  return (
    <div className="grid gap-4 xl:grid-cols-2">
      <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
        <div className="mb-4 flex items-center gap-2">
          <Banknote size={18} className="text-teal-700" />
          <h2 className="font-semibold">Fluxo do mês</h2>
        </div>
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={cashFlow}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="name" />
              <YAxis tickFormatter={(value) => currency.format(Number(value))} width={90} />
              <Tooltip formatter={(value) => currency.format(Number(value))} />
              <Bar dataKey="total" radius={[6, 6, 0, 0]}>
                {cashFlow.map((entry) => (
                  <Cell
                    key={entry.name}
                    fill={entry.name === 'Receitas' ? '#047857' : '#b91c1c'}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
        <h2 className="mb-4 font-semibold">Despesas por categoria</h2>
        <div className="h-72">
          {categoryData.length === 0 ? (
            <div className="flex h-full items-center justify-center text-sm text-slate-500">
              Sem despesas no período.
            </div>
          ) : (
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={categoryData} dataKey="total" nameKey="categoryName" outerRadius={95}>
                  {categoryData.map((entry, index) => (
                    <Cell key={entry.categoryId} fill={colors[index % colors.length]} />
                  ))}
                </Pie>
                <Tooltip formatter={(value) => currency.format(Number(value))} />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>
    </div>
  )
}

function TransactionsTable({ transactions }: { transactions: Transaction[] }) {
  return (
    <div className="overflow-hidden rounded-md border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-200 px-4 py-3">
        <h2 className="font-semibold">Lançamentos do mês</h2>
      </div>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-200 text-sm">
          <thead className="bg-slate-50 text-left text-slate-500">
            <tr>
              <th className="px-4 py-3 font-medium">Data</th>
              <th className="px-4 py-3 font-medium">Descrição</th>
              <th className="px-4 py-3 font-medium">Tipo</th>
              <th className="px-4 py-3 text-right font-medium">Valor</th>
              <th className="px-4 py-3 font-medium">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {transactions.length === 0 ? (
              <tr>
                <td className="px-4 py-8 text-center text-slate-500" colSpan={5}>
                  Nenhum lançamento encontrado.
                </td>
              </tr>
            ) : (
              transactions.map((transaction) => (
                <tr key={transaction.id}>
                  <td className="whitespace-nowrap px-4 py-3">{transaction.date}</td>
                  <td className="min-w-56 px-4 py-3 font-medium text-slate-800">
                    {transaction.description}
                  </td>
                  <td className="px-4 py-3">
                    {transaction.type === 'Income' ? 'Receita' : 'Despesa'}
                  </td>
                  <td
                    className={`whitespace-nowrap px-4 py-3 text-right font-semibold ${
                      transaction.type === 'Income' ? 'text-emerald-700' : 'text-red-700'
                    }`}
                  >
                    {currency.format(transaction.amount)}
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`rounded-full px-2 py-1 text-xs font-medium ${
                        transaction.isPaid
                          ? 'bg-emerald-50 text-emerald-700'
                          : 'bg-amber-50 text-amber-700'
                      }`}
                    >
                      {transaction.isPaid ? 'Pago' : 'Pendente'}
                    </span>
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

export default App
