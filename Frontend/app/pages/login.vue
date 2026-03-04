<template>
  <v-container style="max-width: 420px; padding-top: 80px;">
    <div class="text-center mb-8">
      <h1 class="text-h4 font-weight-bold">
        <span style="color: rgb(var(--v-theme-primary))">Movie</span> Tree
      </h1>
      <p class="text-body-2 mt-2" style="color: rgba(var(--v-theme-on-surface), 0.5)">
        Sign in to your account
      </p>
    </div>

    <v-card color="surface" border rounded="xl" class="pa-6">
      <!-- Tab switcher -->
      <div class="d-flex ga-1 mb-6">
        <v-btn
          v-for="tab in tabs"
          :key="tab.value"
          :variant="mode === tab.value ? 'flat' : 'text'"
          :color="mode === tab.value ? 'primary' : 'default'"
          size="small"
          rounded="pill"
          :style="mode === tab.value ? 'color: rgb(var(--v-theme-on-primary))' : ''"
          style="text-transform: none; font-weight: 500;"
          @click="mode = tab.value"
        >
          {{ tab.label }}
        </v-btn>
      </div>

      <v-form @submit.prevent="submit">
        <v-text-field
          v-model="username"
          label="Username"
          variant="outlined"
          density="compact"
          rounded="lg"
          class="mb-3"
          autocomplete="username"
          :disabled="loading"
        />

        <v-text-field
          v-if="mode === 'register'"
          v-model="email"
          label="Email"
          type="email"
          variant="outlined"
          density="compact"
          rounded="lg"
          class="mb-3"
          autocomplete="email"
          :disabled="loading"
        />

        <v-text-field
          v-model="password"
          label="Password"
          type="password"
          variant="outlined"
          density="compact"
          rounded="lg"
          class="mb-4"
          autocomplete="current-password"
          :disabled="loading"
        />

        <v-alert v-if="error" type="error" variant="tonal" density="compact" class="mb-4">
          {{ error }}
        </v-alert>

        <v-btn
          type="submit"
          color="primary"
          size="large"
          rounded="pill"
          block
          :loading="loading"
          style="color: rgb(var(--v-theme-on-primary)); font-weight: 600; text-transform: none;"
        >
          {{ mode === 'login' ? 'Sign In' : 'Create Account' }}
        </v-btn>
      </v-form>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

const authStore = useAuthStore()

// Redirect if already logged in
if (authStore.isLoggedIn) {
  await navigateTo('/')
}

const mode = ref<'login' | 'register'>('login')
const tabs = [
  { label: 'Sign In', value: 'login' },
  { label: 'Register', value: 'register' },
] as const

const username = ref('')
const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref<string | null>(null)

const submit = async () => {
  error.value = null
  loading.value = true
  try {
    if (mode.value === 'login') {
      await authStore.login(username.value, password.value)
    } else {
      await authStore.register(username.value, email.value, password.value)
    }
    await navigateTo('/')
  } catch (e: unknown) {
    const msg = (e as { data?: { message?: string } })?.data?.message
    error.value = msg ?? 'Something went wrong. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>
