<template>
  <v-container fluid class="pa-6">

    <!-- Header + Regenerate button -->
    <div class="d-flex align-center justify-space-between mb-8 flex-wrap ga-3">
      <div>
        <h2 class="text-h5 font-weight-bold mb-1">Your Recommendations</h2>
        <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.5)">
          Refreshes daily — hit Regenerate for a new batch any time.
        </p>
      </div>
      <v-btn
        color="primary"
        rounded="pill"
        :loading="generateStore.isGenerating"
        style="text-transform: none; font-weight: 600; color: rgb(var(--v-theme-on-primary));"
        @click="regenerate"
      >
        <v-icon start>mdi-refresh</v-icon>
        Regenerate
      </v-btn>
    </div>

    <!-- Results -->
    <v-row v-if="generateStore.picks.length" dense>
      <v-col
        v-for="(movie, index) in generateStore.picks"
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

    <!-- Loading skeleton -->
    <v-row v-else-if="generateStore.isGenerating" dense>
      <v-col v-for="n in 10" :key="n" cols="6" sm="4" md="3" lg="2">
        <v-skeleton-loader type="card" rounded="lg" />
      </v-col>
    </v-row>

    <!-- Empty / error state -->
    <div v-else class="d-flex flex-column align-center justify-center py-16 text-center">
      <v-icon size="72" style="color: rgba(var(--v-theme-on-surface), 0.15)" class="mb-4">
        mdi-creation-outline
      </v-icon>
      <h3 class="text-h6 font-weight-semibold mb-2">No recommendations yet</h3>
      <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.5); max-width: 340px;">
        Hit Regenerate to get 10 movie picks.
      </p>
    </div>

  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const generateStore = useGenerateStore()

await generateStore.loadPicks()

async function regenerate() {
  await generateStore.fetchFresh()
}
</script>

<style scoped>
.result-number {
  position: absolute;
  top: 6px;
  left: 6px;
  z-index: 1;
  background: rgba(var(--v-theme-primary), 0.85);
  color: rgb(var(--v-theme-on-primary));
  width: 22px;
  height: 22px;
  border-radius: 50%;
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>


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
