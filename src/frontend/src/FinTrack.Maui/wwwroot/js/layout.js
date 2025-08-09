// Layout utilities for responsive behavior
let dotNetHelper = null;

export function addResizeListener(dotNetObjectReference) {
    dotNetHelper = dotNetObjectReference;
    
    // Add resize event listener
    window.addEventListener('resize', handleResize);
    
    // Initial call to set the correct state
    handleResize();
}

function handleResize() {
    if (dotNetHelper) {
        const width = window.innerWidth;
        dotNetHelper.invokeMethodAsync('OnWindowResize', width);
    }
}

// Cleanup function
export function removeResizeListener() {
    if (dotNetHelper) {
        window.removeEventListener('resize', handleResize);
        dotNetHelper = null;
    }
}

// Utility function to check if device is mobile
export function isMobileDevice() {
    return window.innerWidth < 960;
}

// Utility function to get current breakpoint
export function getCurrentBreakpoint() {
    const width = window.innerWidth;
    if (width < 600) return 'xs';
    if (width < 960) return 'sm';
    if (width < 1280) return 'md';
    if (width < 1920) return 'lg';
    return 'xl';
}