<template>
  <div class="conversation-area">
    <div class="messages" ref="messagesContainer">
      <div 
        v-for="(message, index) in messages" 
        :key="index" 
        class="message"
        :class="message.role"
      >
        <div class="message-content">{{ message.content }}</div>
      </div>
      <div v-if="isLoading" class="message assistant loading">
        <div class="typing-indicator">
          <span></span>
          <span></span>
          <span></span>
        </div>
      </div>
    </div>
    <div class="message-input">
      <textarea 
        v-model="userInput" 
        placeholder="Type your question..." 
        @keydown.enter.prevent="sendMessage"
        ref="inputField"
      ></textarea>
      <button 
        class="send-button" 
        @click="sendMessage" 
        :disabled="!userInput.trim() || isLoading"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="22" y1="2" x2="11" y2="13"></line>
          <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
        </svg>
      </button>
    </div>
  </div>
</template>

<script>
import { ref, watch, nextTick } from 'vue';

export default {
  name: 'ConversationArea',
  props: {
    messages: {
      type: Array,
      required: true
    },
    isLoading: {
      type: Boolean,
      default: false
    }
  },
  setup(props, { emit }) {
    const userInput = ref('');
    const messagesContainer = ref(null);
    const inputField = ref(null);
    
    // Scroll to bottom when messages change
    watch(() => props.messages.length, async () => {
      await nextTick();
      scrollToBottom();
    });
    
    // Scroll to bottom when loading state changes
    watch(() => props.isLoading, async () => {
      await nextTick();
      scrollToBottom();
    });
    
    // Focus input field when component is mounted
    watch(() => inputField.value, (newValue) => {
      if (newValue) {
        newValue.focus();
      }
    });
    
    const scrollToBottom = () => {
      if (messagesContainer.value) {
        messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
      }
    };
    
    const sendMessage = () => {
      const trimmedInput = userInput.value.trim();
      if (trimmedInput && !props.isLoading) {
        emit('send-message', trimmedInput);
        userInput.value = '';
      }
    };
    
    return {
      userInput,
      messagesContainer,
      inputField,
      sendMessage
    };
  }
};
</script>

<style scoped>
.conversation-area {
  display: flex;
  flex-direction: column;
  height: calc(100% - 60px);
  background-color: #f8f9fa;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.message {
  max-width: 80%;
  padding: 10px 14px;
  border-radius: 16px;
  line-height: 1.4;
  font-size: 14px;
  position: relative;
  word-wrap: break-word;
}

.message.user {
  align-self: flex-end;
  background-color: #4a6cf7;
  color: white;
  border-bottom-right-radius: 4px;
}

.message.assistant {
  align-self: flex-start;
  background-color: #e9ecef;
  color: #212529;
  border-bottom-left-radius: 4px;
}

.message-content {
  white-space: pre-wrap;
}

.message-input {
  display: flex;
  padding: 12px;
  background-color: white;
  border-top: 1px solid #dee2e6;
}

textarea {
  flex: 1;
  border: 1px solid #dee2e6;
  border-radius: 20px;
  padding: 10px 14px;
  resize: none;
  height: 40px;
  font-family: inherit;
  font-size: 14px;
  outline: none;
}

textarea:focus {
  border-color: #4a6cf7;
}

.send-button {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-color: #4a6cf7;
  color: white;
  border: none;
  margin-left: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.send-button:disabled {
  background-color: #adb5bd;
  cursor: not-allowed;
}

.typing-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 4px;
}

.typing-indicator span {
  width: 8px;
  height: 8px;
  background-color: #adb5bd;
  border-radius: 50%;
  display: inline-block;
  animation: bounce 1.4s infinite ease-in-out both;
}

.typing-indicator span:nth-child(1) {
  animation-delay: -0.32s;
}

.typing-indicator span:nth-child(2) {
  animation-delay: -0.16s;
}

@keyframes bounce {
  0%, 80%, 100% {
    transform: scale(0);
  }
  40% {
    transform: scale(1);
  }
}
</style>
