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
      </div>

      <!-- Menu dropdown -->
      <v-menu location="bottom end" :offset="8" :close-on-content-click="true">
        <template #activator="{ props: menuProps }">
          <v-btn
            v-bind="menuProps"
            variant="text"
            icon
            size="small"
            class="ml-1 mr-2"
          >
            <v-icon>mdi-menu</v-icon>
          </v-btn>
        </template>

        <v-list min-width="180" rounded="lg" bg-color="surface" density="compact" class="py-1">
          <!-- Signed-in header -->
          <template v-if="authStore.isLoggedIn">
            <v-list-item class="px-4 pt-2 pb-1" :ripple="false" style="cursor: default">
              <div class="d-flex align-center ga-2">
                <v-avatar size="28" color="primary">
                  <span class="text-caption font-weight-bold" style="color: rgb(var(--v-theme-on-primary))">
                    {{ authStore.user?.userName?.charAt(0).toUpperCase() }}
                  </span>
                </v-avatar>
                <span class="text-body-2 font-weight-medium">{{ authStore.user?.userName }}</span>
              </div>
            </v-list-item>
            <v-divider class="mb-1" />
            <v-list-item
              prepend-icon="mdi-account-outline"
              title="Profile"
              to="/profile"
              rounded="lg"
              class="mx-1"
            />
          </template>

          <v-list-item
            prepend-icon="mdi-cog-outline"
            title="Settings"
            to="/settings"
            rounded="lg"
            class="mx-1"
          />

          <template v-if="authStore.isLoggedIn">
            <v-divider class="my-1" />
            <v-list-item
              prepend-icon="mdi-logout"
              title="Sign out"
              rounded="lg"
              class="mx-1"
              style="color: rgba(var(--v-theme-on-surface), 0.7)"
              @click="logout"
            />
          </template>

          <template v-else>
            <v-divider class="my-1" />
            <v-list-item
              prepend-icon="mdi-login"
              title="Sign in"
              to="/login"
              rounded="lg"
              class="mx-1"
            />
          </template>
        </v-list>
      </v-menu>
    </v-app-bar>

    <v-main>
      <slot />
    </v-main>

    <MaybeMovieModal />
  </div>
</template>

<script setup lang="ts">
import { defineComponent, getCurrentInstance, h, resolveComponent } from 'vue'
import MovieModalFallback from '~/components/MovieModal.vue'

const route = useRoute()
const authStore = useAuthStore()

const MaybeMovieModal = defineComponent({
  name: 'MaybeMovieModal',
  setup() {
    const inst = getCurrentInstance()

    return () => {
      const registered = Boolean(
        inst?.appContext?.components?.MovieModal || inst?.appContext?.components?.['movie-modal']
      )
      if (registered) return h(resolveComponent('MovieModal') as any)
      return h(MovieModalFallback as any)
    }
  },
})

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
