import React, { useMemo, useState } from 'react';
import {
  ArrowLeft,
  Bell,
  Camera,
  CheckCircle2,
  ChevronRight,
  Download,
  Edit3,
  FileText,
  Filter,
  HeartPulse,
  Home,
  Info,
  LogOut,
  MapPin,
  Menu,
  PawPrint,
  Plane,
  Plus,
  QrCode,
  Search,
  Settings,
  ShieldCheck,
  Sparkles,
  Syringe,
  Trash2,
  User,
  X,
} from 'lucide-react';
import './app.css';

type Screen =
  | 'login'
  | 'register'
  | 'home'
  | 'pets'
  | 'petForm'
  | 'petDetail'
  | 'vaccines'
  | 'travel'
  | 'vets'
  | 'settings'
  | 'about';

type Pet = {
  id: string;
  name: string;
  species: string;
  breed: string;
  birth: string;
  sex: string;
  color: string;
  microchip: string;
  father: string;
  mother: string;
  birthplace: string;
  tutor: string;
  cpf: string;
  email: string;
  phone: string;
  cep: string;
  address: string;
  city: string;
  state: string;
  allergies: string;
  notes: string;
};

type Vaccine = {
  id: string;
  petId: string;
  name: string;
  date: string;
  nextDate: string;
  clinic: string;
  veterinarian: string;
  batch: string;
};

type UserProfile = {
  name: string;
  email: string;
  password: string;
  phone: string;
  city: string;
};

const STORAGE_KEYS = {
  user: 'petid_user_profile',
  pets: 'petid_pets',
  vaccines: 'petid_vaccines',
};

const defaultUser: UserProfile = {
  name: 'Gabriel Souza',
  email: 'gabriel.gsouzza6@gmail.com',
  password: 'Pet@12345',
  phone: '(48) 99999-0000',
  city: 'Criciúma, SC',
};

const defaultPets: Pet[] = [
  {
    id: 'kurt',
    name: 'Kurt',
    species: 'Cachorro',
    breed: 'Bulldog Inglês',
    birth: '12/03/2021',
    sex: 'Macho',
    color: 'Dourado',
    microchip: '985141000123456',
    father: 'Nome do Pai do Pet',
    mother: 'Nome da Mãe do Pet',
    birthplace: 'Criciúma',
    tutor: 'Gabriel Souza',
    cpf: '000.000.000-00',
    email: 'gabriel.gsouzza6@gmail.com',
    phone: '(48) 99999-0000',
    cep: '88813-343',
    address: 'Rua Brasília',
    city: 'Criciúma',
    state: 'SC',
    allergies: 'Sem alergias registradas',
    notes: 'Pet dócil, acostumado com viagens e eventos pet-friendly.',
  },
  {
    id: 'beleza',
    name: 'Beleza',
    species: 'Gato',
    breed: 'Bengal',
    birth: '02/08/2022',
    sex: 'Fêmea',
    color: 'Caramelo',
    microchip: '985141000654321',
    father: 'Não informado',
    mother: 'Não informado',
    birthplace: 'Criciúma',
    tutor: 'Gabriel Souza',
    cpf: '000.000.000-00',
    email: 'gabriel.gsouzza6@gmail.com',
    phone: '(48) 99999-0000',
    cep: '88813-343',
    address: 'Rua Brasília',
    city: 'Criciúma',
    state: 'SC',
    allergies: 'Sensível a ração com corante',
    notes: 'Gosta de caixa de transporte ventilada.',
  },
];

const defaultVaccines: Vaccine[] = [
  {
    id: 'v1',
    petId: 'kurt',
    name: 'V10',
    date: '10/01/2026',
    nextDate: '10/01/2027',
    clinic: 'Clínica Animal Care',
    veterinarian: 'Dra. Marina Lopes',
    batch: 'BR-V10-2026',
  },
  {
    id: 'v2',
    petId: 'kurt',
    name: 'Antirrábica',
    date: '18/02/2026',
    nextDate: '18/02/2027',
    clinic: 'Pet Saúde Criciúma',
    veterinarian: 'Dr. Rafael Martins',
    batch: 'RAIVA-0226',
  },
];

const clinics = [
  { name: 'Clínica Animal Care', distance: '1,2 km', rating: '4,9', open: 'Aberta até 20h', address: 'Av. Centenário, 1200', phone: '(48) 3433-1000' },
  { name: 'Pet Saúde Criciúma', distance: '2,4 km', rating: '4,8', open: 'Vacinação hoje', address: 'Rua Henrique Lage, 330', phone: '(48) 3437-2020' },
  { name: 'Hospital Veterinário Sul', distance: '4,1 km', rating: '4,7', open: 'Plantão 24h', address: 'Rua João Pessoa, 91', phone: '(48) 3445-4040' },
];

function readStorage<T>(key: string, fallback: T): T {
  const raw = localStorage.getItem(key);
  if (!raw) return fallback;
  try {
    return JSON.parse(raw) as T;
  } catch {
    return fallback;
  }
}

function PawLogo({ large = false }: { large?: boolean }) {
  return (
    <div className={large ? 'paw-logo paw-logo-large' : 'paw-logo'}>
      <PawPrint size={large ? 112 : 42} strokeWidth={2.4} />
    </div>
  );
}

function EmptyState({ title, description, icon }: { title: string; description: string; icon: React.ReactNode }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">{icon}</div>
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
}

function BottomNav({ screen, go }: { screen: Screen; go: (screen: Screen) => void }) {
  const items = [
    { screen: 'home' as Screen, label: 'Início', icon: Home },
    { screen: 'pets' as Screen, label: 'Meus Pets', icon: PawPrint },
    { screen: 'vaccines' as Screen, label: 'Vacinas', icon: Syringe },
    { screen: 'settings' as Screen, label: 'Ajustes', icon: Settings },
  ];

  return (
    <nav className="bottom-nav" aria-label="Navegação principal">
      {items.map((item) => {
        const Icon = item.icon;
        const active = screen === item.screen || (item.screen === 'pets' && ['petDetail', 'petForm'].includes(screen));
        return (
          <button className={active ? 'nav-item active' : 'nav-item'} key={item.screen} onClick={() => go(item.screen)}>
            <Icon size={24} />
            <span>{item.label}</span>
          </button>
        );
      })}
    </nav>
  );
}

function App(): JSX.Element {
  const [screen, setScreen] = useState<Screen>('login');
  const [user, setUser] = useState<UserProfile>(() => readStorage(STORAGE_KEYS.user, defaultUser));
  const [pets, setPets] = useState<Pet[]>(() => readStorage(STORAGE_KEYS.pets, defaultPets));
  const [vaccines, setVaccines] = useState<Vaccine[]>(() => readStorage(STORAGE_KEYS.vaccines, defaultVaccines));
  const [selectedPetId, setSelectedPetId] = useState(defaultPets[0].id);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const selectedPet = pets.find((pet) => pet.id === selectedPetId) ?? pets[0];
  const isAuthScreen = screen === 'login' || screen === 'register';

  const navigate = (next: Screen) => {
    setDrawerOpen(false);
    setScreen(next);
  };

  const persistUser = (next: UserProfile) => {
    setUser(next);
    localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(next));
  };

  const persistPets = (next: Pet[]) => {
    setPets(next);
    localStorage.setItem(STORAGE_KEYS.pets, JSON.stringify(next));
  };

  const persistVaccines = (next: Vaccine[]) => {
    setVaccines(next);
    localStorage.setItem(STORAGE_KEYS.vaccines, JSON.stringify(next));
  };

  const savePet = (pet: Pet) => {
    const exists = pets.some((item) => item.id === pet.id);
    const next = exists ? pets.map((item) => (item.id === pet.id ? pet : item)) : [pet, ...pets];
    persistPets(next);
    setSelectedPetId(pet.id);
    setScreen('petDetail');
  };

  const deletePet = (petId: string) => {
    const next = pets.filter((pet) => pet.id !== petId);
    persistPets(next);
    persistVaccines(vaccines.filter((vaccine) => vaccine.petId !== petId));
    setSelectedPetId(next[0]?.id ?? '');
    setScreen('pets');
  };

  const addVaccine = (vaccine: Omit<Vaccine, 'id'>) => {
    persistVaccines([{ ...vaccine, id: crypto.randomUUID() }, ...vaccines]);
  };

  if (isAuthScreen) {
    return (
      <main className="app-shell auth-shell">
        {screen === 'login' ? (
          <LoginScreen user={user} onLogin={() => navigate('home')} onRegister={() => navigate('register')} />
        ) : (
          <RegisterScreen
            onBack={() => navigate('login')}
            onCreate={(nextUser) => {
              persistUser(nextUser);
              navigate('home');
            }}
          />
        )}
      </main>
    );
  }

  return (
    <main className="app-shell">
      <Drawer open={drawerOpen} user={user} go={navigate} close={() => setDrawerOpen(false)} />
      {screen === 'home' && <HomeScreen user={user} pets={pets} go={navigate} openMenu={() => setDrawerOpen(true)} />}
      {screen === 'pets' && <PetsScreen pets={pets} go={navigate} openMenu={() => setDrawerOpen(true)} selectPet={setSelectedPetId} />}
      {screen === 'petForm' && <PetFormScreen user={user} pet={selectedPet} onBack={() => navigate('pets')} onSave={savePet} isEditing={Boolean(selectedPet)} />}
      {screen === 'petDetail' && selectedPet && <PetDetailScreen pet={selectedPet} go={navigate} onDelete={deletePet} />}
      {screen === 'vaccines' && <VaccinesScreen pets={pets} vaccines={vaccines} selectedPet={selectedPet} selectPet={setSelectedPetId} go={navigate} addVaccine={addVaccine} />}
      {screen === 'travel' && <TravelScreen pet={selectedPet} go={navigate} />}
      {screen === 'vets' && <VetsScreen go={navigate} />}
      {screen === 'settings' && <SettingsScreen user={user} setUser={persistUser} go={navigate} logout={() => navigate('login')} />}
      {screen === 'about' && <AboutScreen go={navigate} openMenu={() => setDrawerOpen(true)} />}
      <BottomNav screen={screen} go={navigate} />
    </main>
  );
}

function LoginScreen({ user, onLogin, onRegister }: { user: UserProfile; onLogin: () => void; onRegister: () => void }) {
  const [email, setEmail] = useState(user.email);
  const [password, setPassword] = useState('');

  return (
    <section className="auth-card screen-fade">
      <PawLogo large />
      <h1>Identificação Pet</h1>
      <p>Carteira digital completa para seu melhor amigo.</p>
      <label>
        Email
        <input value={email} onChange={(event) => setEmail(event.target.value)} placeholder="seu@email.com" type="email" />
      </label>
      <label>
        Senha
        <input value={password} onChange={(event) => setPassword(event.target.value)} placeholder="••••••••" type="password" />
      </label>
      <button className="primary-button" onClick={onLogin}>Acessar</button>
      <button className="link-button" onClick={onRegister}>Crie a sua conta</button>
      <p className="install-hint"><Download size={16} /> Instale no Android ou iOS pelo navegador.</p>
    </section>
  );
}

function RegisterScreen({ onBack, onCreate }: { onBack: () => void; onCreate: (user: UserProfile) => void }) {
  const [form, setForm] = useState<UserProfile>({ name: '', email: '', password: '', phone: '', city: '' });
  const set = (field: keyof UserProfile, value: string) => setForm((current) => ({ ...current, [field]: value }));

  return (
    <section className="auth-card screen-fade">
      <button className="back-button floating" onClick={onBack}><ArrowLeft /></button>
      <PawLogo large />
      <h1>Criar conta</h1>
      <label>Nome<input value={form.name} onChange={(e) => set('name', e.target.value)} placeholder="Seu nome" /></label>
      <label>Email<input value={form.email} onChange={(e) => set('email', e.target.value)} placeholder="seu@email.com" /></label>
      <label>Senha<input value={form.password} onChange={(e) => set('password', e.target.value)} placeholder="Mínimo 6 caracteres" type="password" /></label>
      <div className="password-rules">
        <strong>A senha deve conter:</strong>
        <span>✓ mínimo de 6 caracteres</span>
        <span>✓ uma letra maiúscula e minúscula</span>
        <span>✓ um número ou caractere especial</span>
      </div>
      <button className="primary-button" onClick={() => onCreate({ ...defaultUser, ...form })}>Criar Conta</button>
    </section>
  );
}

function TopHeader({ title, subtitle, openMenu }: { title: string; subtitle?: string; openMenu?: () => void }) {
  return (
    <header className="top-header">
      {openMenu && <button className="icon-button" onClick={openMenu}><Menu /></button>}
      <div>
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
      </div>
      <button className="icon-button"><Bell /></button>
    </header>
  );
}

function HomeScreen({ user, pets, go, openMenu }: { user: UserProfile; pets: Pet[]; go: (screen: Screen) => void; openMenu: () => void }) {
  const nextVaccine = '10/01/2027';
  return (
    <section className="screen page-with-nav screen-fade">
      <TopHeader title={`Olá, ${user.name.split(' ')[0] || 'bem-vindo'}!`} subtitle="Tudo o que seu pet precisa em um só lugar" openMenu={openMenu} />
      <button className="hero-card" onClick={() => go('pets')}>
        <div><strong>Meus Pets</strong><span>Acesse as carteirinhas</span></div>
        <PawPrint size={58} />
      </button>
      <div className="dashboard-grid">
        <button className="feature-card dark" onClick={() => go('pets')}><PawPrint /><strong>Lista de Pets</strong><span>{pets.length} cadastrados</span></button>
        <button className="feature-card dark" onClick={() => go('vaccines')}><Syringe /><strong>Vacinações</strong><span>Próxima: {nextVaccine}</span></button>
        <button className="feature-card dark" onClick={() => go('travel')}><Plane /><strong>Viajar com Pet</strong><span>Checklist e documentos</span></button>
        <button className="feature-card dark" onClick={() => go('vets')}><MapPin /><strong>Veterinárias</strong><span>Próximas da sua casa</span></button>
      </div>
      <section className="health-card">
        <div><HeartPulse /><strong>Resumo de saúde</strong></div>
        <p>Carteira, vacinas, tutor, microchip, QR Code e orientações de viagem centralizados.</p>
      </section>
    </section>
  );
}

function PetsScreen({ pets, go, openMenu, selectPet }: { pets: Pet[]; go: (screen: Screen) => void; openMenu: () => void; selectPet: (id: string) => void }) {
  const [query, setQuery] = useState('');
  const [filterOpen, setFilterOpen] = useState(false);
  const [sort, setSort] = useState('name');
  const [species, setSpecies] = useState('Todos');

  const filteredPets = useMemo(() => {
    return pets
      .filter((pet) => `${pet.name} ${pet.breed} ${pet.species}`.toLowerCase().includes(query.toLowerCase()))
      .filter((pet) => species === 'Todos' || pet.species === species)
      .sort((a, b) => (sort === 'breed' ? a.breed.localeCompare(b.breed) : a.name.localeCompare(b.name)));
  }, [pets, query, sort, species]);

  return (
    <section className="screen page-with-nav screen-fade">
      <TopHeader title="Meus Pets" subtitle="Gerencie seus amigos peludos" openMenu={openMenu} />
      <div className="search-bar">
        <Search />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Procurar pet..." />
        <button onClick={() => setFilterOpen(true)}><Filter /></button>
      </div>
      <div className="pet-grid">
        {filteredPets.map((pet) => (
          <button
            className="pet-card"
            key={pet.id}
            onClick={() => {
              selectPet(pet.id);
              go('petDetail');
            }}
          >
            <PawLogo />
            <strong>{pet.name}</strong>
            <span>{pet.breed}</span>
          </button>
        ))}
      </div>
      {!filteredPets.length && <EmptyState icon={<PawPrint />} title="Nenhum pet encontrado" description="Cadastre ou ajuste os filtros para ver os pets." />}
      <button className="fab" onClick={() => { selectPet(''); go('petForm'); }}><Plus /></button>
      {filterOpen && (
        <div className="modal-backdrop">
          <div className="filter-modal">
            <h2>Filtrar e Ordenar Pets</h2>
            <label>Ordenação<select value={sort} onChange={(e) => setSort(e.target.value)}><option value="name">Ordem Alfabética (Nome)</option><option value="breed">Filtrar por Raça</option></select></label>
            <label>Espécie<select value={species} onChange={(e) => setSpecies(e.target.value)}><option>Todos</option><option>Cachorro</option><option>Gato</option><option>Pássaro</option></select></label>
            <div className="modal-actions"><button onClick={() => { setSpecies('Todos'); setSort('name'); }}>Limpar Filtros</button><button onClick={() => setFilterOpen(false)}>Cancelar</button><button onClick={() => setFilterOpen(false)}>Aplicar</button></div>
          </div>
        </div>
      )}
    </section>
  );
}

function PetFormScreen({ user, pet, onBack, onSave, isEditing }: { user: UserProfile; pet?: Pet; onBack: () => void; onSave: (pet: Pet) => void; isEditing: boolean }) {
  const blank: Pet = {
    id: crypto.randomUUID(), name: '', species: 'Cachorro', breed: '', birth: '', sex: 'Macho', color: '', microchip: '', father: '', mother: '', birthplace: '', tutor: user.name, cpf: '', email: user.email, phone: user.phone, cep: '', address: '', city: user.city, state: 'SC', allergies: '', notes: '',
  };
  const [form, setForm] = useState<Pet>(isEditing && pet ? pet : blank);
  const set = (field: keyof Pet, value: string) => setForm((current) => ({ ...current, [field]: value }));

  const fields: Array<[keyof Pet, string, string?]> = [
    ['name', 'Nome do Pet*'], ['birth', 'Nascimento'], ['breed', 'Raça*'], ['color', 'Cor predominante'], ['microchip', 'Microchip'], ['father', 'Pai'], ['mother', 'Mãe'], ['birthplace', 'Naturalidade'], ['tutor', 'Nome do tutor'], ['cpf', 'CPF do tutor'], ['email', 'E-mail'], ['phone', 'Telefone'], ['cep', 'CEP'], ['address', 'Endereço'], ['city', 'Cidade'], ['state', 'Estado'], ['allergies', 'Alergias'], ['notes', 'Descrição e observações'],
  ];

  return (
    <section className="screen form-screen screen-fade">
      <button className="back-button" onClick={onBack}><ArrowLeft /></button>
      <PawLogo large />
      <h1>{isEditing ? 'Editar Pet' : 'Cadastrar Pet'}</h1>
      <p>Informações completas para identificação, saúde, viagem e eventos.</p>
      <div className="form-grid compact">
        <label>Espécie<select value={form.species} onChange={(e) => set('species', e.target.value)}><option>Cachorro</option><option>Gato</option><option>Pássaro</option><option>Outro</option></select></label>
        <label>Sexo<select value={form.sex} onChange={(e) => set('sex', e.target.value)}><option>Macho</option><option>Fêmea</option><option>Não informado</option></select></label>
      </div>
      {fields.map(([field, label]) => (
        <label key={field}>{label}<input value={form[field]} onChange={(e) => set(field, e.target.value)} placeholder={label} /></label>
      ))}
      <button className="primary-button sticky-save" onClick={() => onSave(form)}><CheckCircle2 /> Salvar carteira</button>
    </section>
  );
}

function PetDetailScreen({ pet, go, onDelete }: { pet: Pet; go: (screen: Screen) => void; onDelete: (id: string) => void }) {
  const [tab, setTab] = useState<'identity' | 'details'>('identity');
  return (
    <section className="screen pet-detail screen-fade">
      <div className="pet-hero">
        <button className="back-button light" onClick={() => go('pets')}><ArrowLeft /></button>
        <PawPrint className="hero-paw" />
        <h1>{pet.name}</h1>
        <p>{pet.breed}</p>
      </div>
      <div className="tabs"><button className={tab === 'identity' ? 'active' : ''} onClick={() => setTab('identity')}>Identidade</button><button className={tab === 'details' ? 'active' : ''} onClick={() => setTab('details')}>Detalhes</button></div>
      {tab === 'identity' ? <IdentityCard pet={pet} /> : <PetFacts pet={pet} />}
      <button className="soft-button" onClick={() => go('vaccines')}><Syringe /> Carteirinha de Vacinação</button>
      <button className="primary-button" onClick={() => go('petForm')}><Edit3 /> Editar Pet</button>
      <button className="danger-button" onClick={() => onDelete(pet.id)}><Trash2 /> Deletar Pet</button>
    </section>
  );
}

function IdentityCard({ pet }: { pet: Pet }) {
  return (
    <article className="identity-card">
      <div className="card-kicker">República Federativa dos Animais</div>
      <h2>Brasil<br />Carteira de Identidade Animal</h2>
      <div className="card-body">
        <div className="photo-box"><Camera /><span>Foto do pet</span></div>
        <div className="qr-box"><QrCode size={96} /><span>ID #{pet.id.slice(0, 8).toUpperCase()}</span></div>
      </div>
      <div className="signature-line">Assinatura digital</div>
      <div className="valid-line">Válido em todo território nacional</div>
    </article>
  );
}

function PetFacts({ pet }: { pet: Pet }) {
  const rows = [
    ['Nome', pet.name], ['Nascimento', pet.birth], ['Espécie', pet.species], ['Raça', pet.breed], ['Sexo', pet.sex], ['Cor', pet.color], ['Microchip', pet.microchip], ['Tutor', pet.tutor], ['CEP', pet.cep], ['Cidade/UF', `${pet.city}/${pet.state}`], ['Telefone', pet.phone], ['E-mail', pet.email], ['Alergias', pet.allergies], ['Descrição', pet.notes],
  ];
  return <article className="facts-card">{rows.map(([label, value]) => <div key={label}><strong>{label}</strong><span>{value || 'Não informado'}</span></div>)}</article>;
}

function VaccinesScreen({ pets, vaccines, selectedPet, selectPet, go, addVaccine }: { pets: Pet[]; vaccines: Vaccine[]; selectedPet?: Pet; selectPet: (id: string) => void; go: (screen: Screen) => void; addVaccine: (vaccine: Omit<Vaccine, 'id'>) => void }) {
  const [showForm, setShowForm] = useState(false);
  const petVaccines = vaccines.filter((vaccine) => vaccine.petId === selectedPet?.id);
  const [form, setForm] = useState({ name: '', date: '', nextDate: '', clinic: '', veterinarian: '', batch: '' });
  const set = (field: keyof typeof form, value: string) => setForm((current) => ({ ...current, [field]: value }));

  return (
    <section className="screen page-with-nav screen-fade">
      <button className="back-button" onClick={() => go('home')}><ArrowLeft /></button>
      <div className="section-title"><Syringe /><div><h1>Carteira de Vacinação</h1><p>Histórico de imunização do seu pet</p></div></div>
      <select className="pet-select" value={selectedPet?.id ?? ''} onChange={(e) => selectPet(e.target.value)}>{pets.map((pet) => <option key={pet.id} value={pet.id}>{pet.name}</option>)}</select>
      <div className="vaccine-list">
        {petVaccines.map((vaccine) => <article className="vaccine-card" key={vaccine.id}><div><strong>{vaccine.name}</strong><span>{vaccine.clinic}</span></div><div><small>Aplicada</small><b>{vaccine.date}</b></div><div><small>Próxima</small><b>{vaccine.nextDate}</b></div><p>Lote {vaccine.batch} • {vaccine.veterinarian}</p></article>)}
      </div>
      {!petVaccines.length && <EmptyState icon={<Syringe />} title="Nenhuma vacina registrada" description="Adicione vacinas para não perder prazos importantes." />}
      {showForm && <div className="quick-form">{(['name', 'date', 'nextDate', 'clinic', 'veterinarian', 'batch'] as const).map((field) => <input key={field} value={form[field]} onChange={(e) => set(field, e.target.value)} placeholder={{ name: 'Vacina', date: 'Data', nextDate: 'Próxima dose', clinic: 'Clínica', veterinarian: 'Veterinário', batch: 'Lote' }[field]} />)}<button className="primary-button" onClick={() => { if (selectedPet) addVaccine({ ...form, petId: selectedPet.id }); setShowForm(false); }}>Salvar vacina</button></div>}
      <button className="fab" onClick={() => setShowForm((current) => !current)}><Syringe /></button>
    </section>
  );
}

function TravelScreen({ pet, go }: { pet?: Pet; go: (screen: Screen) => void }) {
  const checklist = ['Carteira de vacinação atualizada', 'Atestado sanitário do veterinário', 'Identificação com QR Code', 'Caixa de transporte adequada', 'Água, ração e medicação de uso contínuo'];
  return (
    <section className="screen screen-fade page-with-nav">
      <button className="back-button" onClick={() => go('home')}><ArrowLeft /></button>
      <div className="section-title"><Plane /><div><h1>Viajar com Pet</h1><p>Documentos e checklist para {pet?.name ?? 'seu pet'}</p></div></div>
      <article className="travel-pass"><ShieldCheck /><h2>Passaporte Pet Digital</h2><p>Use esta área para organizar documentos necessários antes de viagens, hotéis e eventos pet-friendly.</p></article>
      <div className="checklist">{checklist.map((item) => <label key={item}><input type="checkbox" /> <span>{item}</span></label>)}</div>
      <button className="primary-button"><FileText /> Gerar resumo para viagem</button>
    </section>
  );
}

function VetsScreen({ go }: { go: (screen: Screen) => void }) {
  return (
    <section className="screen screen-fade page-with-nav">
      <button className="back-button" onClick={() => go('home')}><ArrowLeft /></button>
      <div className="section-title"><MapPin /><div><h1>Veterinárias próximas</h1><p>Locais para consulta, vacinação e emergência</p></div></div>
      <div className="map-card"><MapPin size={48} /><strong>Mapa inteligente</strong><span>Permita localização no navegador para calcular rotas reais.</span></div>
      {clinics.map((clinic) => <article className="clinic-card" key={clinic.name}><div><h2>{clinic.name}</h2><p>{clinic.address}</p><span>{clinic.open}</span></div><strong>{clinic.distance}</strong><small>★ {clinic.rating} • {clinic.phone}</small></article>)}
    </section>
  );
}

function SettingsScreen({ user, setUser, go, logout }: { user: UserProfile; setUser: (user: UserProfile) => void; go: (screen: Screen) => void; logout: () => void }) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(user);
  const set = (field: keyof UserProfile, value: string) => setDraft((current) => ({ ...current, [field]: value }));
  return (
    <section className="screen settings-screen page-with-nav screen-fade">
      <button className="back-button light" onClick={() => go('home')}><ArrowLeft /></button>
      <h1>Configurações</h1>
      <article className="profile-card"><div className="avatar"><User size={70} /></div><h2>Informações do Perfil</h2>{(['name', 'email', 'password', 'phone', 'city'] as const).map((field) => <label key={field}>{field === 'name' ? 'Nome' : field === 'password' ? 'Senha' : field === 'phone' ? 'Telefone' : field === 'city' ? 'Cidade' : 'Email'}<input disabled={!editing} type={field === 'password' ? 'password' : 'text'} value={draft[field]} onChange={(e) => set(field, e.target.value)} /></label>)}</article>
      <button className="success-button" onClick={() => { if (editing) setUser(draft); setEditing(!editing); }}><Edit3 /> {editing ? 'Salvar Perfil' : 'Editar Perfil'}</button>
      <button className="warning-button"><ShieldCheck /> Alterar Senha</button>
      <button className="info-button" onClick={() => go('about')}><Info /> Sobre o App</button>
      <button className="danger-button" onClick={logout}><LogOut /> Sair da Conta</button>
      <p className="version">Versão 2.0.0 • PWA</p>
    </section>
  );
}

function AboutScreen({ go, openMenu }: { go: (screen: Screen) => void; openMenu: () => void }) {
  return (
    <section className="screen page-with-nav about-screen screen-fade">
      <TopHeader title="Identificação Pet" subtitle="Carteira digital para pets" openMenu={openMenu} />
      <article className="about-card"><Sparkles /><h2>Seu pet seguro em todos os momentos</h2><p>Aplicação web instalável para Android e iOS, criada para centralizar identidade, tutor, saúde, vacinas, QR Code, veterinárias próximas e checklist de viagem.</p></article>
      <article className="about-text"><h3>Principais características</h3><p>• Carteira de identificação virtual com dados completos do pet e do tutor.</p><p>• Carteirinha de vacinação digital com histórico e próxima dose.</p><p>• Área de viagem com documentos importantes e checklist.</p><p>• Lista de clínicas veterinárias próximas para vacinação e emergências.</p><p>• Visual responsivo, moderno e pensado para qualquer celular.</p></article>
      <button className="primary-button" onClick={() => go('home')}>Começar agora</button>
    </section>
  );
}

function Drawer({ open, user, go, close }: { open: boolean; user: UserProfile; go: (screen: Screen) => void; close: () => void }) {
  return (
    <div className={open ? 'drawer-layer open' : 'drawer-layer'}>
      <button className="drawer-scrim" onClick={close} aria-label="Fechar menu" />
      <aside className="drawer">
        <button className="drawer-close" onClick={close}><X /></button>
        <div className="drawer-profile"><div className="avatar small"><User /></div><div><strong>{user.name}</strong><span>{user.email}</span></div></div>
        <button onClick={() => go('settings')}><Settings /> Configurações <ChevronRight /></button>
        <button onClick={() => go('about')}><Info /> Sobre o Identificação Pet <ChevronRight /></button>
        <button onClick={() => go('pets')}><PawPrint /> Menu Pets <ChevronRight /></button>
        <button onClick={() => go('vets')}><MapPin /> Veterinárias próximas <ChevronRight /></button>
        <button onClick={() => go('travel')}><Plane /> Viajar com Pet <ChevronRight /></button>
        <button className="drawer-logout" onClick={() => go('login')}><LogOut /> Sair</button>
      </aside>
    </div>
  );
}

export default App;
