import { Banknote } from 'lucide-react'
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
import type { MonthlySummary } from '../api'

const currency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const colors = ['#0f766e', '#2563eb', '#7c3aed', '#db2777', '#ea580c', '#65a30d']

export default function Charts({ summary }: { summary: MonthlySummary | null }) {
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
          <h2 className="font-semibold">Fluxo do mes</h2>
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
                  <Cell key={entry.name} fill={entry.name === 'Receitas' ? '#047857' : '#b91c1c'} />
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
              Sem despesas no periodo.
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
