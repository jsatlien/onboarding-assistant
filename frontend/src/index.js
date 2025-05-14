import AssistantWidget from './components/AssistantWidget.vue';

// Export the components
export { AssistantWidget };

// Create install function
export function install(app) {
  // Register components
  app.component('AssistantWidget', AssistantWidget);
}

// Create module definition for Vue.use()
export default {
  install
};
