// ---------------------------------------------------------------------------
// Mock API — an in-browser stand-in for the ASP.NET Core backend.
// Used while VITE_USE_MOCK=true (default) so the UI is fully demoable before
// the .NET API is built. It mirrors the exact endpoints the real API will expose.
// ---------------------------------------------------------------------------

const DB_KEY = 'tm_mock_db'
const SESSION_KEY = 'tm_mock_session'
const LATENCY = 350

const seed = () => {
  const now = Date.now()
  const day = 24 * 60 * 60 * 1000
  return {
    users: [
      { id: 1, name: 'Gul Fatima', email: 'admin@example.com', password: 'Admin@123', role: 'Admin', createdAt: now - 30 * day },
      { id: 2, name: 'Demo User', email: 'user@example.com', password: 'User@123', role: 'User', createdAt: now - 20 * day },
    ],
    categories: [
      { id: 1, name: 'Work' },
      { id: 2, name: 'Personal' },
      { id: 3, name: 'Urgent' },
      { id: 4, name: 'Learning' },
    ],
    tasks: [
      { id: 1, title: 'Set up .NET solution', description: 'Create the API, core, infrastructure and test projects.', status: 'Completed', priority: 'High', dueDate: new Date(now - 5 * day).toISOString(), categoryId: 1, assignedUserId: 1, createdAt: now - 12 * day, updatedAt: now - 5 * day },
      { id: 2, title: 'Design database schema', description: 'Users, tasks and categories tables with relationships.', status: 'Completed', priority: 'Medium', dueDate: new Date(now - 3 * day).toISOString(), categoryId: 1, assignedUserId: 1, createdAt: now - 10 * day, updatedAt: now - 3 * day },
      { id: 3, title: 'Build React frontend', description: 'Vite + React SPA with routing, auth and task screens.', status: 'InProgress', priority: 'High', dueDate: new Date(now + 4 * day).toISOString(), categoryId: 1, assignedUserId: 1, createdAt: now - 2 * day, updatedAt: now - 1 * day },
      { id: 4, title: 'Write unit tests', description: 'xUnit tests for controllers, services and repositories.', status: 'Pending', priority: 'Medium', dueDate: new Date(now + 7 * day).toISOString(), categoryId: 4, assignedUserId: 1, createdAt: now - 1 * day, updatedAt: now - 1 * day },
      { id: 5, title: 'Grocery shopping', description: 'Milk, eggs, bread and coffee.', status: 'Pending', priority: 'Low', dueDate: new Date(now + 2 * day).toISOString(), categoryId: 2, assignedUserId: 2, createdAt: now - 2 * day, updatedAt: now - 2 * day },
      { id: 6, title: 'Pay electricity bill', description: 'Due before the 15th.', status: 'InProgress', priority: 'High', dueDate: new Date(now + 1 * day).toISOString(), categoryId: 3, assignedUserId: 2, createdAt: now - 4 * day, updatedAt: now - 1 * day },
      { id: 7, title: 'Finish React course', description: 'Complete the final module on hooks.', status: 'Pending', priority: 'Medium', dueDate: new Date(now + 10 * day).toISOString(), categoryId: 4, assignedUserId: 2, createdAt: now - 3 * day, updatedAt: now - 3 * day },
      { id: 8, title: 'Team sync meeting', description: 'Weekly stand-up with the cohort.', status: 'Completed', priority: 'Low', dueDate: new Date(now - 1 * day).toISOString(), categoryId: 1, assignedUserId: 1, createdAt: now - 6 * day, updatedAt: now - 1 * day },
    ],
    nextUserId: 3,
    nextTaskId: 9,
  }
}

const loadDb = () => {
  const raw = localStorage.getItem(DB_KEY)
  if (!raw) {
    const db = seed()
    localStorage.setItem(DB_KEY, JSON.stringify(db))
    return db
  }
  try {
    return JSON.parse(raw)
  } catch {
    const db = seed()
    localStorage.setItem(DB_KEY, JSON.stringify(db))
    return db
  }
}

const saveDb = (db) => localStorage.setItem(DB_KEY, JSON.stringify(db))

const delay = (ms = LATENCY) => new Promise((resolve) => setTimeout(resolve, ms))

// --- fake JWT ----------------------------------------------------------------
const b64 = (obj) => btoa(unescape(encodeURIComponent(JSON.stringify(obj))))

const signToken = (user) => {
  const header = b64({ alg: 'HS256', typ: 'JWT' })
  const payload = b64({ sub: user.id, name: user.name, email: user.email, role: user.role, exp: Date.now() + 24 * 3600 * 1000 })
  return `${header}.${payload}.mock-signature`
}

const publicUser = (u) => ({ id: u.id, name: u.name, email: u.email, role: u.role, createdAt: u.createdAt })

const getSessionUser = (db) => {
  const id = Number(localStorage.getItem(SESSION_KEY))
  if (!id) return null
  return db.users.find((u) => u.id === id) ?? null
}

const httpError = (status, message) => {
  const err = new Error(message)
  err.status = status
  return err
}

const api = {
  // --- auth ------------------------------------------------------------------
  async register({ name, email, password }) {
    await delay()
    const db = loadDb()
    if (!name?.trim() || !email?.trim() || !password?.trim()) {
      throw httpError(400, 'Name, email and password are required.')
    }
    if (db.users.some((u) => u.email.toLowerCase() === email.toLowerCase())) {
      throw httpError(409, 'An account with this email already exists.')
    }
    const user = {
      id: db.nextUserId++,
      name: name.trim(),
      email: email.trim().toLowerCase(),
      password,
      role: 'User',
      createdAt: Date.now(),
    }
    db.users.push(user)
    saveDb(db)
    localStorage.setItem(SESSION_KEY, String(user.id))
    return { token: signToken(user), user: publicUser(user) }
  },

  async login({ email, password }) {
    await delay()
    const db = loadDb()
    const user = db.users.find(
      (u) => u.email.toLowerCase() === email?.trim().toLowerCase() && u.password === password,
    )
    if (!user) throw httpError(401, 'Invalid email or password.')
    localStorage.setItem(SESSION_KEY, String(user.id))
    return { token: signToken(user), user: publicUser(user) }
  },

  async getMe() {
    await delay(150)
    const db = loadDb()
    const user = getSessionUser(db)
    if (!user) throw httpError(401, 'Not authenticated.')
    return publicUser(user)
  },

  // --- tasks ------------------------------------------------------------------
  async getTasks(filters = {}) {
    await delay()
    const db = loadDb()
    let tasks = [...db.tasks]

    const { status, priority, categoryId, assignedUserId, search, dueDate } = filters
    if (status) tasks = tasks.filter((t) => t.status === status)
    if (priority) tasks = tasks.filter((t) => t.priority === priority)
    if (categoryId) tasks = tasks.filter((t) => t.categoryId === Number(categoryId))
    if (assignedUserId) tasks = tasks.filter((t) => t.assignedUserId === Number(assignedUserId))
    if (dueDate) tasks = tasks.filter((t) => t.dueDate.slice(0, 10) === String(dueDate).slice(0, 10))
    if (search) {
      const q = search.toLowerCase()
      tasks = tasks.filter(
        (t) =>
          t.title.toLowerCase().includes(q) ||
          (t.description ?? '').toLowerCase().includes(q),
      )
    }

    // Normalize date ordering: pending/in-progress first, then by due date.
    const order = { Pending: 0, InProgress: 1, Completed: 2 }
    tasks.sort((a, b) => order[a.status] - order[b.status] || a.dueDate.localeCompare(b.dueDate))

    return tasks.map((t) => this.decorate(t, db))
  },

  async getTask(id) {
    await delay()
    const db = loadDb()
    const task = db.tasks.find((t) => t.id === Number(id))
    if (!task) throw httpError(404, 'Task not found.')
    return this.decorate(task, db)
  },

  async createTask(data) {
    await delay()
    const db = loadDb()
    const user = getSessionUser(db)
    if (!user) throw httpError(401, 'Not authenticated.')
    const now = Date.now()
    const task = {
      id: db.nextTaskId++,
      title: data.title?.trim(),
      description: data.description?.trim() ?? '',
      status: data.status || 'Pending',
      priority: data.priority || 'Medium',
      dueDate: data.dueDate ? new Date(data.dueDate).toISOString() : null,
      categoryId: Number(data.categoryId),
      assignedUserId: data.assignedUserId ? Number(data.assignedUserId) : user.id,
      createdAt: now,
      updatedAt: now,
    }
    if (!task.title) throw httpError(400, 'Title is required.')
    db.tasks.push(task)
    saveDb(db)
    return this.decorate(task, db)
  },

  async updateTask(id, data) {
    await delay()
    const db = loadDb()
    const task = db.tasks.find((t) => t.id === Number(id))
    if (!task) throw httpError(404, 'Task not found.')
    const user = getSessionUser(db)
    if (!user) throw httpError(401, 'Not authenticated.')
    // Regular users may only edit their own tasks.
    if (user.role !== 'Admin' && task.assignedUserId !== user.id) {
      throw httpError(403, 'You can only edit your own tasks.')
    }
    Object.assign(task, {
      title: data.title?.trim() ?? task.title,
      description: data.description?.trim() ?? task.description,
      status: data.status ?? task.status,
      priority: data.priority ?? task.priority,
      dueDate: data.dueDate ? new Date(data.dueDate).toISOString() : data.dueDate === '' ? null : task.dueDate,
      categoryId: data.categoryId ? Number(data.categoryId) : task.categoryId,
      assignedUserId: data.assignedUserId ? Number(data.assignedUserId) : task.assignedUserId,
      updatedAt: Date.now(),
    })
    saveDb(db)
    return this.decorate(task, db)
  },

  async deleteTask(id) {
    await delay()
    const db = loadDb()
    const idx = db.tasks.findIndex((t) => t.id === Number(id))
    if (idx === -1) throw httpError(404, 'Task not found.')
    const user = getSessionUser(db)
    if (!user) throw httpError(401, 'Not authenticated.')
    if (user.role !== 'Admin' && db.tasks[idx].assignedUserId !== user.id) {
      throw httpError(403, 'You can only delete your own tasks.')
    }
    db.tasks.splice(idx, 1)
    saveDb(db)
    return { ok: true }
  },

  async getDashboardStats() {
    await delay()
    const db = loadDb()
    const user = getSessionUser(db)
    // Regular users see only their own task counts; Admins see team-wide stats.
    const tasks =
      user?.role === 'Admin'
        ? db.tasks
        : db.tasks.filter((t) => t.assignedUserId === user?.id)
    const stats = { completed: 0, inProgress: 0, pending: 0, total: 0 }
    for (const t of tasks) {
      stats.total++
      if (t.status === 'Completed') stats.completed++
      else if (t.status === 'InProgress') stats.inProgress++
      else stats.pending++
    }
    return stats
  },

  // --- categories ---------------------------------------------------------------
  async getCategories() {
    await delay(150)
    return loadDb().categories
  },

  // --- users --------------------------------------------------------------------
  async getUsers() {
    await delay(150)
    return loadDb().users.map((u) => publicUser(u))
  },

  // --- helpers --------------------------------------------------------------------
  decorate(task, db) {
    const category = db.categories.find((c) => c.id === task.categoryId)
    const assignee = db.users.find((u) => u.id === task.assignedUserId)
    return {
      ...task,
      category: category?.name ?? 'Uncategorized',
      assignedTo: assignee ? { id: assignee.id, name: assignee.name } : null,
    }
  },
}

export default api
