<template>
  <div 
    class="assistant-widget" 
    :class="{ 'expanded': isExpanded }"
    :style="{ top: position.y + 'px', left: position.x + 'px' }"
    ref="widgetRef"
  >
    <div class="assistant-header" @mousedown="startDrag">
      <div class="assistant-avatar">
        <img src="../assets/assistant-avatar.svg" alt="Assistant" />
      </div>
      <div v-if="isExpanded" class="assistant-title">{{ assistantName }}</div>
      <div class="assistant-controls">
        <button @click="toggleExpand" class="toggle-button">
          {{ isExpanded ? '−' : '+' }}
        </button>
      </div>
    </div>
    
    <ConversationArea 
      v-if="isExpanded" 
      :messages="messages" 
      :isLoading="isLoading"
      @send-message="handleSendMessage"
    />
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted, watch } from 'vue';
import ConversationArea from './ConversationArea.vue';
import Shepherd from 'shepherd.js';
import axios from 'axios';

export default {
  name: 'AssistantWidget',
  components: {
    ConversationArea
  },
  props: {
    apiBaseUrl: {
      type: String,
      default: 'http://localhost:5000/api'
    },
    assistantName: {
      type: String,
      default: 'Onboarding Assistant'
    },
    initialPosition: {
      type: Object,
      default: () => ({ x: 20, y: 20 })
    }
  },
  setup(props) {
    // State
    const isExpanded = ref(false);
    const position = ref({ ...props.initialPosition });
    const widgetRef = ref(null);
    
    // Initialize messages from sessionStorage if available, otherwise use default welcome message
    const storedMessages = sessionStorage.getItem('assistantMessages');
    const messages = ref(
      storedMessages 
        ? JSON.parse(storedMessages) 
        : [{ role: 'assistant', content: `Hi there! I'm ${props.assistantName}. How can I help you today?` }]
    );
    
    const isLoading = ref(false);
    const currentRoute = ref(window.location.pathname);
    const threadId = ref(sessionStorage.getItem('assistantThreadId') || '');
    
    // Dragging state
    const isDragging = ref(false);
    const dragOffset = ref({ x: 0, y: 0 });
    
    // Shepherd tour instance
    const tour = ref(null);
    
    // Watch for changes to messages and save to sessionStorage
    watch(() => messages.value, (newMessages) => {
      sessionStorage.setItem('assistantMessages', JSON.stringify(newMessages));
    }, { deep: true });
    
    // Initialize Shepherd
    onMounted(() => {
      tour.value = new Shepherd.Tour({
        useModalOverlay: true,
        defaultStepOptions: {
          classes: 'shepherd-theme-custom',
          scrollTo: true
        }
      });
      
      // Get initial context for the current route
      fetchRouteContext();
      
      // Add event listeners for dragging
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', stopDrag);
      
      // Listen for route changes
      window.addEventListener('popstate', handleRouteChange);
    });
    
    onUnmounted(() => {
      // Clean up event listeners
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', stopDrag);
      window.removeEventListener('popstate', handleRouteChange);
      
      // Clean up tour if it exists
      if (tour.value) {
        tour.value.complete();
      }
    });
    
    // Methods
    const toggleExpand = () => {
      isExpanded.value = !isExpanded.value;
    };
    
    const startDrag = (event) => {
      if (widgetRef.value) {
        isDragging.value = true;
        const rect = widgetRef.value.getBoundingClientRect();
        dragOffset.value = {
          x: event.clientX - rect.left,
          y: event.clientY - rect.top
        };
        event.preventDefault();
      }
    };
    
    const handleMouseMove = (event) => {
      if (isDragging.value) {
        position.value = {
          x: event.clientX - dragOffset.value.x,
          y: event.clientY - dragOffset.value.y
        };
      }
    };
    
    const stopDrag = () => {
      isDragging.value = false;
    };
    
    const handleRouteChange = () => {
      currentRoute.value = window.location.pathname;
      fetchRouteContext();
      // Note: We don't reset messages here - conversation persists across routes
    };
    
    const fetchRouteContext = async () => {
      try {
        const response = await axios.get(`${props.apiBaseUrl}/context`, {
          params: { route: currentRoute.value }
        });
        
        // We could add a system message with the context, but we'll keep it internal for now
        console.log('Fetched context:', response.data);
      } catch (error) {
        console.error('Error fetching context:', error);
      }
    };
    
    const handleSendMessage = async (content) => {
      // Add user message to the conversation
      messages.value.push({ role: 'user', content });
      
      // Set loading state
      isLoading.value = true;
      
      try {
        // Send the query to the backend with threadId
        const response = await axios.post(`${props.apiBaseUrl}/query`, {
          query: content,
          route: currentRoute.value,
          threadId: threadId.value
        });
        
        // Store the thread ID if it's returned
        if (response.data.threadId) {
          threadId.value = response.data.threadId;
          sessionStorage.setItem('assistantThreadId', threadId.value);
        }
        
        // Add assistant response to the conversation
        messages.value.push({ role: 'assistant', content: response.data.response });
        
        // Handle any actions from the response
        if (response.data.actions) {
          handleAssistantActions(response.data.actions);
        }
      } catch (error) {
        console.error('Error sending query:', error);
        messages.value.push({ 
          role: 'assistant', 
          content: 'Sorry, I encountered an error while processing your request.' 
        });
      } finally {
        isLoading.value = false;
      }
    };
    
    const handleAssistantActions = (actions) => {
      if (!actions || !Array.isArray(actions)) return;
      
      actions.forEach(action => {
        switch (action.type) {
          case 'highlight':
            highlightElement(action.elementId, action.description);
            break;
          case 'navigate':
            navigateTo(action.route);
            break;
          default:
            console.warn('Unknown action type:', action.type);
        }
      });
    };
    
    const highlightElement = (elementId, description) => {
      const element = document.getElementById(elementId);
      if (!element) {
        console.warn(`Element with ID "${elementId}" not found`);
        return;
      }
      
      // Clear any existing tour
      if (tour.value) {
        tour.value.complete();
      }
      
      // Create a new tour step
      tour.value.addStep({
        id: `highlight-${elementId}`,
        attachTo: {
          element: `#${elementId}`,
          on: 'bottom'
        },
        buttons: [
          {
            text: 'Close',
            action: tour.value.complete
          }
        ],
        title: 'UI Element',
        text: description || `This is the ${elementId} element`
      });
      
      // Start the tour
      tour.value.start();
    };
    
    const navigateTo = (route) => {
      if (route && route !== currentRoute.value) {
        window.location.href = route;
      }
    };
    
    return {
      isExpanded,
      position,
      widgetRef,
      messages,
      isLoading,
      toggleExpand,
      startDrag,
      handleSendMessage
    };
  }
};
</script>

<style scoped>
.assistant-widget {
  position: fixed;
  z-index: 9999;
  background-color: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  width: 60px;
  transition: width 0.3s ease, height 0.3s ease;
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

.assistant-widget.expanded {
  width: 320px;
  height: 480px;
}

.assistant-header {
  display: flex;
  align-items: center;
  padding: 12px;
  background-color: #4a6cf7;
  color: white;
  cursor: move;
  user-select: none;
}

.assistant-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
}

.assistant-avatar img {
  width: 24px;
  height: 24px;
}

.assistant-title {
  flex: 1;
  font-weight: 600;
  font-size: 16px;
}

.assistant-controls {
  display: flex;
}

.toggle-button {
  background: none;
  border: none;
  color: white;
  font-size: 20px;
  cursor: pointer;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.toggle-button:hover {
  background-color: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
}
</style>
