# Pull Request

## 📋 Description
<!-- Provide a brief description of the changes in this PR -->

### What does this PR do?
<!-- Describe the main purpose and functionality of this PR -->

### Related Issue(s)
<!-- Link to related issues using #issue_number -->
Fixes #
Closes #
Related to #

## 🔧 Type of Change
<!-- Mark the relevant option with an [x] -->

- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📚 Documentation update
- [ ] 🎨 UI/UX improvement
- [ ] ⚡ Performance improvement
- [ ] 🔨 Refactoring (no functional changes)
- [ ] 🧪 Test addition or improvement
- [ ] 🔧 Configuration change
- [ ] 🚀 CI/CD improvement

## 🏗️ Architecture Impact
<!-- Mark the relevant components affected -->

### Frontend (.NET MAUI)
- [ ] Views/Pages
- [ ] ViewModels
- [ ] Services
- [ ] Models
- [ ] Converters
- [ ] Styles/Resources
- [ ] Platform-specific code

### Backend
- [ ] API Controllers
- [ ] Services
- [ ] Repositories
- [ ] Models/Entities
- [ ] Database migrations
- [ ] Authentication/Authorization
- [ ] Configuration

### Shared/Core
- [ ] Shared models
- [ ] Interfaces
- [ ] Enums
- [ ] Extensions
- [ ] Utilities

## 🧪 Testing
<!-- Describe the testing performed -->

### Test Coverage
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] UI tests added/updated
- [ ] Manual testing performed

### Testing Checklist
- [ ] All existing tests pass
- [ ] New functionality is covered by tests
- [ ] Edge cases are tested
- [ ] Error handling is tested

### Manual Testing
<!-- Describe manual testing steps performed -->
1. 
2. 
3. 

## 📱 Platform Testing
<!-- Mark platforms where testing was performed -->

- [ ] Android
- [ ] iOS
- [ ] Windows
- [ ] macOS

## 🔍 Code Quality
<!-- Ensure code quality standards are met -->

- [ ] Code follows project coding standards
- [ ] No compiler warnings introduced
- [ ] Code is properly documented
- [ ] Nullable reference types handled appropriately
- [ ] MVVM pattern followed (for frontend changes)
- [ ] Dependency injection used appropriately
- [ ] Error handling implemented
- [ ] Logging added where appropriate

## 📊 Performance Impact
<!-- Describe any performance implications -->

- [ ] No performance impact
- [ ] Performance improved
- [ ] Performance impact assessed and acceptable
- [ ] Performance testing performed

### Performance Notes
<!-- Add details about performance changes if applicable -->

## 🔒 Security Considerations
<!-- Mark if applicable -->

- [ ] No security implications
- [ ] Security review required
- [ ] Authentication/Authorization changes
- [ ] Data validation added/updated
- [ ] Input sanitization implemented

## 📸 Screenshots/Videos
<!-- Add screenshots or videos demonstrating the changes, especially for UI changes -->

### Before
<!-- Screenshots of the current state -->

### After
<!-- Screenshots of the new state -->

## 📝 Migration Notes
<!-- If this PR requires database migrations or configuration changes -->

### Database Changes
- [ ] No database changes
- [ ] Migration scripts included
- [ ] Backward compatible
- [ ] Data migration required

### Configuration Changes
- [ ] No configuration changes
- [ ] New configuration keys added
- [ ] Environment variables updated
- [ ] Documentation updated

## 📋 Checklist
<!-- Ensure all items are completed before requesting review -->

### General
- [ ] PR title is descriptive and follows conventional commit format
- [ ] Code is self-documenting or includes necessary comments
- [ ] No debugging code left in the codebase
- [ ] No hardcoded values (use configuration instead)
- [ ] Error messages are user-friendly
- [ ] Logging is appropriate and not excessive

### Frontend Specific
- [ ] UI is responsive across different screen sizes
- [ ] Accessibility considerations addressed
- [ ] Loading states implemented where appropriate
- [ ] Error states handled gracefully
- [ ] Navigation flows work correctly
- [ ] Data binding is properly implemented
- [ ] Memory leaks avoided (event handlers unsubscribed)

### Backend Specific
- [ ] API endpoints follow RESTful conventions
- [ ] Input validation implemented
- [ ] Proper HTTP status codes returned
- [ ] API documentation updated (if applicable)
- [ ] Database queries are optimized
- [ ] Proper exception handling implemented

## 🔄 Deployment Notes
<!-- Any special deployment considerations -->

- [ ] No special deployment steps required
- [ ] Requires database migration
- [ ] Requires configuration updates
- [ ] Requires cache clearing
- [ ] Requires service restart

### Deployment Steps
<!-- List any special deployment steps if applicable -->
1. 
2. 
3. 

## 👥 Reviewers
<!-- Tag specific reviewers if needed -->

### Required Reviews
- [ ] Code review
- [ ] UI/UX review (for frontend changes)
- [ ] Security review (for security-related changes)
- [ ] Performance review (for performance-critical changes)

### Suggested Reviewers
<!-- @mention specific team members -->

## 📚 Additional Notes
<!-- Any additional information that reviewers should know -->

### Breaking Changes
<!-- Describe any breaking changes and migration path -->

### Future Considerations
<!-- Any follow-up work or considerations for future PRs -->

---

## 📋 Review Checklist (for Reviewers)
<!-- Checklist for reviewers -->

- [ ] Code logic is sound and efficient
- [ ] Code follows project conventions and standards
- [ ] Tests are comprehensive and pass
- [ ] Documentation is adequate
- [ ] Security considerations are addressed
- [ ] Performance impact is acceptable
- [ ] UI/UX changes are intuitive and accessible
- [ ] Error handling is robust
- [ ] No obvious bugs or edge cases missed