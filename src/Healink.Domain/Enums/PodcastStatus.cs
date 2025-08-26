namespace Healink.Domain.Enums;

/// <summary>
/// Status of podcast content
/// </summary>
public enum PodcastStatus
{
    /// <summary>
    /// Draft - not yet published
    /// </summary>
    Draft = 1,
    
    /// <summary>
    /// Under review for content moderation
    /// </summary>
    UnderReview = 2,
    
    /// <summary>
    /// Published and available to users
    /// </summary>
    Published = 3,
    
    /// <summary>
    /// Archived - no longer actively promoted
    /// </summary>
    Archived = 4,
    
    /// <summary>
    /// Removed due to policy violation
    /// </summary>
    Removed = 5,
    
    /// <summary>
    /// Featured content
    /// </summary>
    Featured = 6
}
