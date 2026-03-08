<template>
  <div>
    <v-app-bar flat height="64" color="surface" border="b">
      <!-- Logo -->
      <NuxtLink to="/" class="nav-logo">
        <v-sheet
          width="36"
          height="36"
          rounded="sm"
          color="primary"
          class="d-flex align-center justify-center ml-4 mr-2"
        >
          <v-icon color="on-primary" size="20">mdi-tree</v-icon>
        </v-sheet>
        <span class="nav-title text-h6 font-weight-bold">Movie Tree</span>
      </NuxtLink>

      <v-spacer />

      <!-- Nav Links -->
      <div class="nav-links d-none d-sm-flex align-center ga-1">
        <v-btn
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          variant="text"
          :color="route.path === link.to ? 'primary' : 'default'"
          size="small"
          class="nav-link-btn text-body-2"
          :class="{ 'nav-link-active': route.path === link.to }"
        >
          {{ link.label }}
        </v-btn>

        <!-- Search -->
        <v-btn
          :to="'/search'"
          variant="text"
          :color="route.path === '/search' ? 'primary' : 'default'"
          icon
          size="small"
        >
          <v-icon>mdi-magnify</v-icon>
        </v-btn>
      </div>

      <!-- Settings / Menu -->
      <v-btn
        :to="'/settings'"
        variant="text"
        icon
        size="small"
        class="ml-1"
        :color="route.path === '/settings' ? 'primary' : 'default'"
      >
        <v-icon>mdi-menu</v-icon>
      </v-btn>

      <!-- Auth: logged in -->
      <template v-if="authStore.isLoggedIn">
        <span class="text-body-2 mx-2" style="color: rgba(var(--v-theme-on-surface), 0.6)">
          {{ authStore.user?.userName }}
        </span>
        <v-btn
          variant="text"
          size="small"
          class="mr-2"
          style="text-transform: none;"
          @click="logout"
        >
          Sign out
        </v-btn>
      </template>

      <!-- Auth: logged out -->
      <v-btn
        v-else
        :to="'/login'"
        variant="flat"
        color="primary"
        size="small"
        rounded="pill"
        class="mr-3"
        style="text-transform: none; color: rgb(var(--v-theme-on-primary));"
      >
        Sign in
      </v-btn>
    </v-app-bar>

    <v-main>
      <slot />
    </v-main>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const authStore = useAuthStore()

const navLinks = computed(() => [
  { label: 'Feed', to: '/feed' },
  { label: 'Recommendations', to: '/generate' },
  { label: 'Your Movies', to: '/your-movies' },
  ...(authStore.isAdmin ? [{ label: 'Admin', to: '/admin' }] : []),
])

const logout = () => {
  authStore.logout()
  navigateTo('/')
}
</script>

<style scoped>
.nav-logo {
  display: flex;
  align-items: center;
  text-decoration: none;
  color: inherit;
}

.nav-title {
  color: rgb(var(--v-theme-on-surface));
  letter-spacing: -0.01em;
}

.nav-link-btn {
  font-weight: 500;
  letter-spacing: 0;
  text-transform: none;
}

.nav-link-active {
  border-bottom: 2px solid rgb(var(--v-theme-primary));
  border-radius: 0;
}
</style>
