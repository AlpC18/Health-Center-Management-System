import { Dumbbell } from 'lucide-react'
import CrudPage from '../../components/crud/CrudPage'
import { ProgramForm } from '../../components/crud/Forms'
import { programetApi } from '../../api/api'

export function ProgrametPage() {
  return (
    <CrudPage
      title="Programet"
      subtitle="Menaxhimi i programeve të stërvitjes"
      emptyIcon={Dumbbell}
      api={programetApi}
      FormComponent={ProgramForm}
      idKey="programId"
      searchKeys={['emriProgramit', 'qellimi']}
      columns={[
        { key: 'emriProgramit', label: 'Emri' },
        { key: 'qellimi', label: 'Qëllimi' },
        {
          key: 'kohezgjatjaJave',
          label: 'Kohëzgjatja',
          render: (item) => (item.kohezgjatjaJave ? `${item.kohezgjatjaJave} javë` : '-'),
        },
      ]}
    />
  )
}
