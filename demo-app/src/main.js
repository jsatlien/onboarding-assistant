import { createApp } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';
import App from './App.vue';
import OnboardingAssistant from 'onboarding-assistant-sdk';

// Import views
import Dashboard from './views/Dashboard.vue';
import WorkOrderCreate from './views/WorkOrderCreate.vue';
import WorkOrderList from './views/WorkOrderList.vue';
import RepairPrograms from './views/RepairPrograms.vue';

// Define routes
const routes = [
  { path: '/', redirect: '/dashboard' },
  { path: '/dashboard', component: Dashboard },
  { path: '/workorders/new', component: WorkOrderCreate },
  { path: '/workorders', component: WorkOrderList },
  { path: '/repair-programs', component: RepairPrograms }
];

// Create router
const router = createRouter({
  history: createWebHistory(),
  routes
});

// Create app
const app = createApp(App);

// Use router
app.use(router);

// Use Onboarding Assistant SDK
app.use(OnboardingAssistant);

// Mount app
app.mount('#app');
