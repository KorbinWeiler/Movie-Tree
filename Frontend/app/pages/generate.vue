<template>
  <v-container fluid class="pa-6">

    <!-- Controls -->
    <div class="d-flex flex-column align-center ga-4 mb-8">
      <v-select
        v-model="generationType"
        :items="generationTypes"
        item-title="label"
        item-value="value"
        label="Generation Type"
        variant="outlined"
        density="compact"
        rounded="lg"
        hide-details
        style="max-width: 360px; width: 100%;"
        bg-color="surface"
      />
      <v-btn
        color="primary"
        size="large"
        rounded="pill"
        :loading="generateStore.isGenerating"
        style="min-width: 180px; color: rgb(var(--v-theme-on-primary)); font-weight: 600; text-transform: none;"
        @click="generate"
      >
        <v-icon start>mdi-creation</v-icon>
        Generate
      </v-btn>

      <v-alert
        v-if="generateError"
        type="error"
        variant="tonal"
        density="compact"
        closable
        style="max-width: 360px; width: 100%;"
        @click:close="generateError = null"
      >
        {{ generateError }}
      </v-alert>
    </div>

    <!-- Results -->
    <div v-if="results.length">
      <div class="d-flex align-center justify-space-between mb-4">
        <span class="section-label">GENERATED FOR YOU</span>
        <span class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.4)">
          {{ results.length }} movies
        </span>
      </div>
      <v-row dense>
        <v-col
          v-for="pick in results"
          :key="pick.movie.id"
          cols="6"
          sm="4"
          md="3"
          lg="2"
        >
          <div class="position-relative">
            <div class="result-number">{{ pick.position }}</div>
            <MovieCard :movie="pick.movie" />
          </div>
        </v-col>
      </v-row>
    </div>

    <!-- Empty state (before generating) -->
    <div v-else class="d-flex flex-column align-center justify-center py-16 text-center">
      <v-icon size="72" style="color: rgba(var(--v-theme-on-surface), 0.15)" class="mb-4">
        mdi-creation-outline
      </v-icon>
      <h3 class="text-h6 font-weight-semibold mb-2">Your recommendations will appear here</h3>
      <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.5); max-width: 340px;">
        Choose how you want your list generated, then hit Generate to get 10 personalised picks.
      </p>
    </div>

  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const generateStore = useGenerateStore()
const movieStore = useMovieStore()

try { await movieStore.fetchGenres() } catch { /* ignore */ }

type GenMode = 'AllHistory' | 'Selected' | 'Genre' | 'FullAI'
const generationType = ref<GenMode>('AllHistory')
const selectedGenreId = ref<number | null>(null)
const generateError = ref<string | null>(null)

const generationTypes = [
  { label: 'Based on all my movies', value: 'AllHistory' },
  { label: 'Select specific movies', value: 'Selected' },
  { label: 'By genre', value: 'Genre' },
  { label: 'Let AI decide', value: 'FullAI' },
] as const

const generate = async () => {
  generateError.value = null
  try {
    await generateStore.generate(
      generationType.value,
      undefined,
      generationType.value === 'Genre' ? (selectedGenreId.value ?? undefined) : undefined
    )
  } catch {
    generateError.value = 'Could not generate recommendations. Please try again.'
  }
}

const results = computed(() =>
  generateStore.lastGenerated?.movies ?? []
)
</script>

<style scoped>
.section-label {
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: rgba(var(--v-theme-on-surface), 0.5);
}

.result-number {
  position: absolute;
  top: 8px;
  left: 8px;
  z-index: 1;
  background: rgba(0, 0, 0, 0.65);
  color: rgb(var(--v-theme-primary));
  font-size: 11px;
  font-weight: 700;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  line-height: 1;
}
</style>
