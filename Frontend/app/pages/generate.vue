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
        :loading="generating"
        style="min-width: 180px; color: rgb(var(--v-theme-on-primary)); font-weight: 600; text-transform: none;"
        @click="generate"
      >
        <v-icon start>mdi-creation</v-icon>
        Generate
      </v-btn>
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
          v-for="(movie, index) in results"
          :key="movie.id"
          cols="6"
          sm="4"
          md="3"
          lg="2"
        >
          <div class="position-relative">
            <div class="result-number">{{ index + 1 }}</div>
            <MovieCard :movie="movie" />
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
const generationType = ref('all')
const generating = ref(false)
const results = ref<typeof moviePool>([])

const generationTypes = [
  { label: 'Based on all my movies', value: 'all' },
  { label: 'Select specific movies', value: 'select' },
  { label: 'By genre', value: 'genre' },
  { label: "Let AI decide", value: 'free' },
]

const moviePool = [
  { id: 101, title: 'Harakiri', year: 1962, rating: 8.6 },
  { id: 102, title: 'In the Mood for Love', year: 2000, rating: 8.1 },
  { id: 103, title: 'A Brighter Summer Day', year: 1991, rating: 8.1 },
  { id: 104, title: 'The Conformist', year: 1970, rating: 7.9 },
  { id: 105, title: 'Pickpocket', year: 1959, rating: 7.7 },
  { id: 106, title: 'Aguirre, the Wrath of God', year: 1972, rating: 7.8 },
  { id: 107, title: 'A Man Escaped', year: 1956, rating: 7.9 },
  { id: 108, title: 'Mon Oncle', year: 1958, rating: 7.7 },
  { id: 109, title: 'The Battle of Algiers', year: 1966, rating: 8.1 },
  { id: 110, title: 'Dersu Uzala', year: 1975, rating: 8.0 },
  { id: 111, title: 'The Travelling Players', year: 1975, rating: 7.8 },
  { id: 112, title: 'Nostalghia', year: 1983, rating: 7.9 },
]

const generate = async () => {
  generating.value = true
  // Simulate an async call
  await new Promise((resolve) => setTimeout(resolve, 1200))
  results.value = [...moviePool].sort(() => Math.random() - 0.5).slice(0, 10)
  generating.value = false
}
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
