<template>
  <v-container class="pa-6" style="max-width: 640px">

    <h1 class="text-h5 font-weight-bold mb-6">Settings</h1>

    <!-- Profile Settings -->
    <div class="settings-group mb-6">
      <div class="group-label mb-3">PROFILE</div>
      <v-card color="surface" border rounded="lg" class="settings-card pa-5">
        <div class="d-flex align-center ga-4 mb-5">
          <v-avatar size="64" color="primary">
            <span class="text-h6 font-weight-bold" style="color: rgb(var(--v-theme-on-primary))">
              {{ profile.username.charAt(0).toUpperCase() }}
            </span>
          </v-avatar>
          <div>
            <div class="text-body-1 font-weight-semibold">{{ profile.username }}</div>
            <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">
              {{ profile.email }}
            </div>
          </div>
          <v-spacer />
          <v-btn variant="outlined" size="small" rounded="pill" style="text-transform: none;">
            Edit
          </v-btn>
        </div>
        <v-divider class="mb-5" />
        <v-row dense class="ga-3">
          <v-col cols="12">
            <v-text-field
              v-model="profile.username"
              label="Username"
              variant="outlined"
              density="compact"
              rounded="lg"
              hide-details
              bg-color="background"
            />
          </v-col>
          <v-col cols="12">
            <v-text-field
              v-model="profile.email"
              label="Email"
              type="email"
              variant="outlined"
              density="compact"
              rounded="lg"
              hide-details
              bg-color="background"
            />
          </v-col>
        </v-row>
        <div class="d-flex justify-end mt-4">
          <v-btn color="primary" size="small" rounded="pill" style="text-transform: none; color: rgb(var(--v-theme-on-primary)); font-weight: 600;">
            Save Changes
          </v-btn>
        </div>
      </v-card>
    </div>

    <!-- Privacy Settings -->
    <div class="settings-group mb-6">
      <div class="group-label mb-3">PRIVACY</div>
      <v-card color="surface" border rounded="lg" class="settings-card pa-5">
        <div
          v-for="(setting, index) in privacySettings"
          :key="setting.key"
        >
          <div class="d-flex align-center justify-space-between py-3">
            <div>
              <div class="text-body-2 font-weight-medium">{{ setting.label }}</div>
              <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">
                {{ setting.description }}
              </div>
            </div>
            <div class="d-flex ga-1 ml-4">
              <v-btn
                v-for="opt in privacyOptions"
                :key="opt.value"
                size="x-small"
                :variant="privacy[setting.key] === opt.value ? 'flat' : 'outlined'"
                :color="privacy[setting.key] === opt.value ? 'primary' : 'default'"
                rounded="pill"
                :style="privacy[setting.key] === opt.value ? 'color: rgb(var(--v-theme-on-primary))' : ''"
                style="text-transform: none; min-width: 68px;"
                @click="privacy[setting.key] = opt.value"
              >
                {{ opt.label }}
              </v-btn>
            </div>
          </div>
          <v-divider v-if="index < privacySettings.length - 1" />
        </div>
      </v-card>
    </div>

    <!-- Account Settings -->
    <div class="settings-group">
      <div class="group-label mb-3">ACCOUNT</div>
      <v-card color="surface" border rounded="lg" class="settings-card pa-5">
        <div class="d-flex align-center justify-space-between py-2">
          <div>
            <div class="text-body-2 font-weight-medium">Change Password</div>
            <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">
              Update your account password
            </div>
          </div>
          <v-btn variant="outlined" size="small" rounded="pill" style="text-transform: none;">
            Change
          </v-btn>
        </div>
        <v-divider />
        <div class="d-flex align-center justify-space-between py-2 mt-1">
          <div>
            <div class="text-body-2 font-weight-medium" style="color: rgb(var(--v-theme-error))">
              Delete Account
            </div>
            <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">
              Permanently remove your account and all data
            </div>
          </div>
          <v-btn color="error" variant="outlined" size="small" rounded="pill" style="text-transform: none;">
            Delete
          </v-btn>
        </div>
      </v-card>
    </div>

  </v-container>
</template>

<script setup lang="ts">
const profile = reactive({
  username: 'cinephile92',
  email: 'user@example.com',
})

const privacyOptions = [
  { label: 'Public', value: 'public' },
  { label: 'Friends', value: 'friends' },
  { label: 'Private', value: 'private' },
]

const privacySettings = [
  {
    key: 'reviews',
    label: 'Review Visibility',
    description: 'Who can see your written reviews',
  },
  {
    key: 'watchlist',
    label: 'Watch Later List',
    description: 'Who can see your watch later list',
  },
]

const privacy = reactive<Record<string, string>>({
  reviews: 'friends',
  watchlist: 'private',
})
</script>

<style scoped>
.settings-card {
  border-color: rgba(var(--v-theme-on-surface), 0.08) !important;
}

.group-label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.07em;
  color: rgba(var(--v-theme-on-surface), 0.45);
}
</style>
