<template>
  <v-container max-width="680" class="py-10">
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
  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'admin' })

const config = useRuntimeConfig()
const authStore = useAuthStore()

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
</script>
