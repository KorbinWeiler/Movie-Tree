<template>
  <v-container max-width="780" class="py-10">

    <!-- Tab bar -->
    <div class="d-flex ga-1 mb-7">
      <v-btn
        v-for="tab in tabs"
        :key="tab.value"
        :variant="activeTab === tab.value ? 'flat' : 'text'"
        :color="activeTab === tab.value ? 'primary' : 'default'"
        size="small"
        rounded="pill"
        :style="activeTab === tab.value ? 'color: rgb(var(--v-theme-on-primary))' : ''"
        style="text-transform: none; font-weight: 500;"
        @click="activeTab = tab.value as 'import' | 'hidden'"
      >
        <v-icon start size="16">{{ tab.icon }}</v-icon>
        {{ tab.label }}
      </v-btn>
    </div>

    <!-- ── Import tab ─────────────────────────────────── -->
    <template v-if="activeTab === 'import'">
    <div class="d-flex align-center ga-3 mb-6">
      <v-icon color="primary" size="28">mdi-database-import</v-icon>
      <span class="text-h5 font-weight-bold">Import Movies</span>
    </div>

    <v-card variant="outlined" rounded="lg" class="pa-6">
      <p class="text-body-2 text-medium-emphasis mb-5">
        Upload a TMDB movie export file (NDJSON format — one JSON object per line).
        Each entry must have an <code>id</code> and <code>original_title</code> field.
        Movies already in the database will be skipped.
      </p>

      <!-- File picker -->
      <v-file-input
        v-model="selectedFile"
        label="Movie JSON file"
        accept=".json"
        prepend-icon="mdi-file-upload"
        variant="outlined"
        density="comfortable"
        :disabled="loading"
        :error-messages="fileError"
        @update:model-value="fileError = ''"
      />

      <!-- Upload button -->
      <v-btn
        color="primary"
        class="mt-4"
        :loading="loading"
        :disabled="!selectedFile || loading"
        @click="upload"
      >
        <v-icon start>mdi-upload</v-icon>
        Upload &amp; Import
      </v-btn>
    </v-card>

    <!-- Result -->
    <v-alert
      v-if="result"
      type="success"
      variant="tonal"
      rounded="lg"
      class="mt-5"
      closable
      @click:close="result = null"
    >
      Import complete — <strong>{{ result.added.toLocaleString() }}</strong> movies added,
      <strong>{{ result.skipped.toLocaleString() }}</strong> skipped (already existed).
    </v-alert>

    <v-alert
      v-if="error"
      type="error"
      variant="tonal"
      rounded="lg"
      class="mt-5"
      closable
      @click:close="error = ''"
    >
      {{ error }}
    </v-alert>
    </template>

    <!-- ── Hidden Movies tab ──────────────────────────── -->
    <template v-if="activeTab === 'hidden'">
      <div class="d-flex align-center ga-3 mb-6">
        <v-icon color="warning" size="28">mdi-eye-off-outline</v-icon>
        <span class="text-h5 font-weight-bold">Hidden Movies</span>
        <v-chip size="small" variant="tonal" color="warning" class="ml-1">{{ hiddenTotal }}</v-chip>
      </div>

      <!-- Search -->
      <v-text-field
        v-model="hiddenQuery"
        prepend-inner-icon="mdi-magnify"
        placeholder="Search hidden movies..."
        variant="outlined"
        density="compact"
        rounded="pill"
        hide-details
        clearable
        class="mb-5"
        bg-color="surface"
        @update:model-value="debouncedHiddenSearch"
      />

      <!-- List -->
      <div v-if="hiddenMovies.length" class="d-flex flex-column ga-2">
        <v-card
          v-for="m in hiddenMovies"
          :key="m.id"
          variant="outlined"
          rounded="lg"
          class="pa-3 d-flex align-center ga-4"
          style="border-color: rgba(var(--v-theme-on-surface), 0.08)"
        >
          <!-- Poster thumbnail -->
          <v-avatar size="44" rounded="sm" color="surface-variant">
            <v-img v-if="m.posterUrl" :src="m.posterUrl" :alt="m.title" cover />
            <v-icon v-else size="20" color="surface-variant">mdi-movie-open</v-icon>
          </v-avatar>

          <div class="flex-grow-1">
            <div class="text-body-2 font-weight-medium">{{ m.title }}</div>
            <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.45)">
              {{ m.releaseDate ? new Date(m.releaseDate).getFullYear() : '' }}
            </div>
          </div>

          <v-btn
            color="success"
            variant="outlined"
            size="small"
            rounded="pill"
            :loading="unhidingId === m.id"
            style="text-transform: none"
            @click="unhideFromPanel(m.id)"
          >
            <v-icon start size="14">mdi-eye-outline</v-icon>
            Show
          </v-btn>
        </v-card>
      </div>

      <!-- Empty state -->
      <div v-else-if="!hiddenLoading" class="d-flex flex-column align-center justify-center py-14 text-center">
        <v-icon size="56" class="mb-3" style="color: rgba(var(--v-theme-on-surface), 0.18)">mdi-eye-check-outline</v-icon>
        <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.45)">No hidden movies.</p>
      </div>

      <div v-if="hiddenLoading" class="d-flex justify-center py-8">
        <v-progress-circular indeterminate color="primary" />
      </div>

      <!-- Pagination -->
      <div v-if="hiddenTotalPages > 1" class="d-flex justify-center mt-6">
        <v-pagination v-model="hiddenPage" :length="hiddenTotalPages" density="compact" rounded="circle" />
      </div>
    </template>

  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'admin' })

const config = useRuntimeConfig()
const authStore = useAuthStore()
const movieStore = useMovieStore()

const tabs = [
  { label: 'Import', value: 'import', icon: 'mdi-database-import' },
  { label: 'Hidden Movies', value: 'hidden', icon: 'mdi-eye-off-outline' },
]
const activeTab = ref<'import' | 'hidden'>('import')

// ── Import tab state ────────────────────────────────────────────────────────
const selectedFile = ref<File | null>(null)
const loading = ref(false)
const fileError = ref('')
const result = ref<{ added: number; skipped: number } | null>(null)
const error = ref('')

async function upload() {
  if (!selectedFile.value) {
    fileError.value = 'Please select a file.'
    return
  }

  result.value = null
  error.value = ''
  loading.value = true

  try {
    const form = new FormData()
    form.append('file', selectedFile.value)

    result.value = await $fetch<{ added: number; skipped: number }>(
      `${config.public.apiBase}/admin/import-movies`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${authStore.token}` },
        body: form,
      }
    )
    selectedFile.value = null
  } catch (err: unknown) {
    const e = err as { data?: { message?: string }; message?: string }
    error.value = e?.data?.message ?? e?.message ?? 'Upload failed. Please try again.'
  } finally {
    loading.value = false
  }
}

// ── Hidden Movies tab state ─────────────────────────────────────────────────
type HiddenMovie = { id: number; title: string; posterUrl: string | null; releaseDate: string | null }

const hiddenMovies = ref<HiddenMovie[]>([])
const hiddenTotal = ref(0)
const hiddenPage = ref(1)
const hiddenQuery = ref('')
const hiddenLoading = ref(false)
const unhidingId = ref<number | null>(null)

const PAGE_SIZE = 30
const hiddenTotalPages = computed(() => Math.ceil(hiddenTotal.value / PAGE_SIZE))

async function loadHidden() {
  hiddenLoading.value = true
  try {
    const res = await movieStore.fetchHiddenMovies(hiddenQuery.value || undefined, hiddenPage.value)
    hiddenMovies.value = res.movies
    hiddenTotal.value = res.total
  } finally {
    hiddenLoading.value = false
  }
}

const debouncedHiddenSearch = useDebounceFn(() => {
  hiddenPage.value = 1
  loadHidden()
}, 300)

watch(activeTab, (tab) => { if (tab === 'hidden') loadHidden() })
watch(hiddenPage, loadHidden)

async function unhideFromPanel(id: number) {
  unhidingId.value = id
  try {
    await movieStore.setMovieVisibility(id, true)
    hiddenMovies.value = hiddenMovies.value.filter(m => m.id !== id)
    hiddenTotal.value = Math.max(0, hiddenTotal.value - 1)
  } finally {
    unhidingId.value = null
  }
}
</script>
