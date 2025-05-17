import AssistantWidget from './components/AssistantWidget.vue';

// Export the components
export { AssistantWidget };

// Create install function
export function install(Vue) {
  // Check if Vue.use() was called with Vue 2 or Vue 3
  if (Vue.component) {
    // Vue 2
    Vue.component('AssistantWidget', AssistantWidget);
  } else if (Vue.app?.component) {
    // Vue 3
    Vue.app.component('AssistantWidget', AssistantWidget);
  } else {
    // Assume Vue 3 directly
    Vue.component('AssistantWidget', AssistantWidget);
  }
}

// Create module definition for Vue.use()
export default {
  install
};
